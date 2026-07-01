# Spec: Steam API Circuit Breaker

## Goal

Steam API 호출(`SteamAuthTicketValidator`)에 Polly v8 Circuit Breaker를 적용하여, Steam API 장애 시 스레드 고갈 없이 즉시 503을 반환한다.

---

## Decisions

| 항목 | 결정 |
|---|---|
| Retry 여부 | 없음 |
| CB 라이브러리 | `Microsoft.Extensions.Http.Resilience` (Polly v8) |
| 상태 저장 | 인메모리 (단일 인스턴스 가정) |
| 파라미터 위치 | `appsettings.json` → `Steam:CircuitBreaker` 섹션 |
| Open 시 응답 | 503 + Korean `CommonApiResponse` |
| Timeout | resilience pipeline 내부 timeout 사용 |
| 상태 변화 로깅 | `ILogger` (`Open`: Warning, `HalfOpen/Closed`: Information) |
| 테스트 | DI로 구성한 typed `HttpClient` + mock `HttpMessageHandler`로 임계치 동작 검증 |

---

## Circuit Breaker Parameters

```json
// appsettings.json
"Steam": {
  "CircuitBreaker": {
    "FailureRatio": 0.5,
    "SamplingDurationSeconds": 30,
    "MinimumThroughput": 5,
    "BreakDurationSeconds": 60,
    "RequestTimeoutSeconds": 10
  }
}
```

| 파라미터 | 값 | 의미 |
|---|---|---|
| `FailureRatio` | 0.5 | 샘플링 창 내 50% 이상 실패 시 Open |
| `SamplingDurationSeconds` | 30 | 실패율 측정 시간 창 |
| `MinimumThroughput` | 5 | Open 전환 전 최소 요청 수 |
| `BreakDurationSeconds` | 60 | Open 유지 시간 (이후 Half-Open) |
| `RequestTimeoutSeconds` | 10 | Steam API 단일 요청 timeout |

Half-Open에서 허용할 프로브 요청 수: 1 (Polly v8 기본값)

> 주의: Circuit Breaker는 실패 비율과 최소 처리량 조건을 만족한 **실패 응답 자체**를 `BrokenCircuitException`으로 바꾸지 않는다. 해당 요청 결과를 기준으로 회로가 Open되고, 그 다음 요청부터 즉시 `BrokenCircuitException`이 발생한다.

---

## Files to Change

### 1. NuGet 패키지 추가

```xml
<!-- PushAndPull/PushAndPull.csproj -->
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="9.0.12" />
```

프로젝트의 다른 .NET 9 패키지처럼 exact version으로 고정한다. wildcard 버전(`9.*`)은 restore 시점에 따라 결과가 바뀔 수 있으므로 사용하지 않는다.

### 2. 새 예외 클래스

**`PushAndPull/Domain/Auth/Exception/SteamCircuitOpenException.cs`** (신규)

```csharp
namespace PushAndPull.Domain.Auth.Exception;

public class SteamCircuitOpenException(System.Exception innerException)
    : SteamApiException("STEAM_API_CIRCUIT_OPEN", innerException);
```

`SteamApiException`을 상속하면 기존 예외 계층을 유지하면서 503 매핑을 위한 타입 분기가 가능하다.

### 3. 설정 바인딩 클래스

**`PushAndPull/Domain/Auth/Config/CircuitBreakerOptions.cs`** (신규)

```csharp
namespace PushAndPull.Domain.Auth.Config;

public class CircuitBreakerOptions
{
    public double FailureRatio { get; init; } = 0.5;
    public int SamplingDurationSeconds { get; init; } = 30;
    public int MinimumThroughput { get; init; } = 5;
    public int BreakDurationSeconds { get; init; } = 60;
    public int RequestTimeoutSeconds { get; init; } = 10;
}
```

설정값 검증 규칙:

- `FailureRatio`: `0 < value <= 1`
- `SamplingDurationSeconds`, `BreakDurationSeconds`, `RequestTimeoutSeconds`: `> 0`
- `MinimumThroughput`: `>= 2` (Polly v8 요구사항)

검증은 `AddAuthServices` 내부에서 바인딩 직후에 수행한다. 잘못된 설정은 애플리케이션 시작 시점에 `ArgumentException`으로 실패시킨다.

```csharp
// AddAuthServices 내부, CB 등록 이전
var cbOptions = configuration
    .GetSection("Steam:CircuitBreaker")
    .Get<CircuitBreakerOptions>() ?? new CircuitBreakerOptions();

if (cbOptions.FailureRatio is <= 0 or > 1)
    throw new ArgumentException("FailureRatio must be between 0 (exclusive) and 1 (inclusive).");
if (cbOptions.SamplingDurationSeconds <= 0)
    throw new ArgumentException("SamplingDurationSeconds must be greater than 0.");
if (cbOptions.BreakDurationSeconds <= 0)
    throw new ArgumentException("BreakDurationSeconds must be greater than 0.");
if (cbOptions.RequestTimeoutSeconds <= 0)
    throw new ArgumentException("RequestTimeoutSeconds must be greater than 0.");
if (cbOptions.MinimumThroughput < 2)
    throw new ArgumentException("MinimumThroughput must be at least 2 (Polly v8 requirement).");
```

### 4. AuthServiceConfig 수정

**`PushAndPull/Domain/Auth/Config/AuthServiceConfig.cs`**

`AddHttpClient` 호출에 `AddResilienceHandler`를 체이닝한다.

```csharp
using Microsoft.Extensions.Http.Resilience;
using Polly.CircuitBreaker;
using Polly.Timeout;

public static IServiceCollection AddAuthServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var cbOptions = configuration
        .GetSection("Steam:CircuitBreaker")
        .Get<CircuitBreakerOptions>() ?? new CircuitBreakerOptions();

    services
        .AddHttpClient<IAuthTicketValidator, SteamAuthTicketValidator>()
        .AddResilienceHandler("steam-cb", (builder, context) =>
        {
            var logger = context.ServiceProvider
                .GetRequiredService<ILogger<SteamAuthTicketValidator>>();

            // Circuit breaker가 timeout 실패도 집계하도록 CB를 timeout 바깥에 둔다.
            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio        = cbOptions.FailureRatio,
                SamplingDuration    = TimeSpan.FromSeconds(cbOptions.SamplingDurationSeconds),
                MinimumThroughput   = cbOptions.MinimumThroughput,
                BreakDuration       = TimeSpan.FromSeconds(cbOptions.BreakDurationSeconds),
                OnOpened = args =>
                {
                    logger.LogWarning(
                        "Steam API circuit breaker opened. Break for {Duration}s.",
                        cbOptions.BreakDurationSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    logger.LogInformation("Steam API circuit breaker closed.");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = _ =>
                {
                    logger.LogInformation("Steam API circuit breaker half-opened.");
                    return ValueTask.CompletedTask;
                }
            });

            builder.AddTimeout(new HttpTimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(cbOptions.RequestTimeoutSeconds)
            });
        });

    services.AddScoped<ISessionService, SessionService>();
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<ILoginService, LoginService>();
    services.AddScoped<ILogoutService, LogoutService>();
    return services;
}
```

`Program.cs`에서 `AddAuthServices(builder.Configuration)` 으로 시그니처 변경.

### 5. SteamAuthTicketValidator 수정

**`PushAndPull/Global/Auth/SteamAuthTicketValidator.cs`**

파일 상단에 using을 추가하고, `ValidateAsync`의 catch 블록에 `BrokenCircuitException`, `TimeoutRejectedException` 처리를 추가한다.

```csharp
// 추가할 using
using Polly.CircuitBreaker;
using Polly.Timeout;
```

```csharp
// 기존
catch (HttpRequestException ex)
{
    throw new SteamApiException("FAIL_TO_CONNECT", ex);
}

// 변경 후 (BrokenCircuitException catch를 HttpRequestException 위에 배치)
catch (BrokenCircuitException ex)
{
    throw new SteamCircuitOpenException(ex);
}
catch (TimeoutRejectedException ex)
{
    throw new SteamApiException("STEAM_API_TIMEOUT", ex);
}
catch (HttpRequestException ex)
{
    throw new SteamApiException("FAIL_TO_CONNECT", ex);
}
```

`BrokenCircuitException` catch는 `HttpRequestException`보다 위에 둔다. `TimeoutRejectedException`은 회로가 아직 Closed인 상태에서 Steam API가 응답하지 않을 때의 사용자 응답을 기존 Steam API 장애 계층으로 맞추기 위한 처리다.

### 6. 503 예외 매핑 필터

Gamism SDK가 예외를 가로채는 방식이 불명확하므로, **커스텀 예외 필터**를 추가한다.

**`PushAndPull/Global/Filter/CircuitBreakerExceptionFilter.cs`** (신규)

```csharp
using Gamism.SDK.Core.Network;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PushAndPull.Domain.Auth.Exception;

namespace PushAndPull.Global.Filter;

public class CircuitBreakerExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not SteamCircuitOpenException)
            return;

        context.Result = new ObjectResult(
            CommonApiResponse.Fail("스팀 인증 서버에 일시적으로 연결할 수 없습니다. 잠시 후 다시 시도해 주세요."))
        {
            StatusCode = StatusCodes.Status503ServiceUnavailable
        };
        context.ExceptionHandled = true;
    }
}
```

**`Program.cs`** 또는 `AddControllers()`에 필터 등록:

```csharp
builder.Services.AddControllers(options =>
    options.Filters.Add<CircuitBreakerExceptionFilter>());
```

---

## Test Plan

**파일**: `PushAndPull.Test/Service/Auth/SteamCircuitBreakerTests.cs`

테스트 대상: DI로 구성한 `IAuthTicketValidator` typed client (실제 `SteamAuthTicketValidator`, mock `HttpMessageHandler` 사용)

중요: `SteamAuthTicketValidator`에 mock handler로 만든 `HttpClient`를 직접 주입하면 `AddResilienceHandler` 파이프라인이 빠진다. 테스트는 `ServiceCollection`에서 `AddAuthServices(configuration)`를 호출하고, typed client의 primary handler만 mock으로 교체해서 실제 resilience handler 체인을 통과시켜야 한다.

| 시나리오 | 검증 내용 |
|---|---|
| `When_FailuresBelowThreshold` → `It_DoesNotOpenCircuit` | MinimumThroughput - 1 번 실패 후 CB가 Open되지 않음. 다음 호출이 실제 HTTP로 나감. |
| `When_FailureRatioExceeded` → `It_OpensCircuitAndNextCallThrowsSteamCircuitOpenException` | 임계치에 도달한 실패 요청 이후 회로가 Open됨. 그 다음 호출은 실제 HTTP로 나가지 않고 `SteamCircuitOpenException` 발생. |
| `When_CircuitOpen_AndBreakDurationElapsed` → `It_AllowsProbeRequest` | Break duration 경과 후 Half-Open 전환. 다음 요청이 실제 HTTP로 나감. |
| `When_SteamApiHangs` → `It_TimesOutAndContributesToCircuitBreaker` | mock handler가 응답하지 않을 때 timeout이 발생하고, 반복 timeout이 CB 실패 집계에 포함됨. |
| `When_CircuitOpenExceptionIsThrown` → `It_Returns503CommonApiResponse` | 로그인 API 또는 예외 필터 테스트에서 `503 + CommonApiResponse.Fail(...)` 반환. |

테스트에서 CB 파라미터는 빠른 검증을 위해 짧은 값(SamplingDuration=2s, BreakDuration=1s, MinimumThroughput=2)으로 오버라이드한다.

---

## Verification Checklist

1. `dotnet build PushAndPull.sln` 통과
2. `dotnet test PushAndPull.sln` 통과
3. `POST /api/v1/auth/login` 호출 시:
   - 정상 → 200
   - CB Open 상태 → 503 + 한국어 메시지
4. appsettings.json 값 변경 후 재기동 시 파라미터 반영 확인
