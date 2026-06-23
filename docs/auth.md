# Auth (인증)

Steam 티켓 기반 인증과 Redis 세션 관리를 정리한다.

## 개요

- 인증 수단은 **Steam 티켓 → 세션 발급 → `Session-Id` 헤더**다. JWT/Bearer는 쓰지 않는다.
- 세션은 Redis에 저장하며 **유저당 단일 세션**을 보장한다.
- 외부 의존성은 Steam Web API 하나이며, Polly 서킷브레이커/타임아웃으로 격리한다.

요청 흐름은 프로젝트 공통 규칙대로 `Controller → Service → Repository(또는 CacheStore)`를 따른다.

## 엔드포인트

| 메서드 | 경로 | 인증 | Rate Limit | 설명 |
|---|---|---|---|---|
| `POST` | `/api/v1/auth/login` | 불필요 | `login` | Steam 티켓 검증 후 세션 발급 |
| `POST` | `/api/v1/auth/logout` | `[SessionAuthorize]` | — | 현재 세션 무효화 |

응답은 모두 `CommonApiResponse`로 감싼다.

### 로그인 `POST /api/v1/auth/login`

요청 본문 (`LoginRequest`):

```json
{ "steamTicket": "<steam auth session ticket>", "nickname": "플레이어닉네임" }
```

성공 응답 (`LoginResponse`):

```json
{ "sessionId": "<guid>" }
```

처리 순서 (`LoginService.ExecuteAsync`):

1. **닉네임 검증** — 공백 불가, 최대 32자. 위반 시 `InvalidNicknameException`.
2. **티켓 검증** — `IAuthTicketValidator.ValidateAsync`로 Steam Web API 호출. SteamId / OwnerSteamId / VAC 밴 / 퍼블리셔 밴을 받는다.
3. **정책 차단**
   - 패밀리 셰어링(`SteamId != OwnerSteamId`) → `FamilySharingNotAllowedException`
   - VAC 밴 → `VacBannedException`
   - 퍼블리셔 밴 → `PublisherBannedException`
4. **유저 upsert** — `IUserRepository`로 SteamId 조회. 없으면 `User` 생성, 있으면 닉네임 + `LastLoginAt` 갱신.
5. **세션 발급** — `ISessionService.CreateAsync(steamId, TTL 15일)`. 반환한 `SessionId`를 클라이언트에 내려준다.

### 로그아웃 `POST /api/v1/auth/logout`

`Session-Id` 헤더로 인증한 뒤, 클레임에서 꺼낸 세션 ID를 `ISessionService.DeleteAsync`로 무효화한다.

## Steam 티켓 검증 (`SteamAuthTicketValidator`)

- `IAuthTicketValidator` 뒤에 숨긴다(보안 규칙). 검증 로직을 컨트롤러/서비스에 직접 두지 않는다.
- 호출 대상: `ISteamUserAuth/AuthenticateUserTicket/v1`. `Steam:WebApiKey`(키 이름 → 실제 키)와 `Steam:AppId`를 설정에서 읽는다.
- 응답 검증: `Result == "OK"` 확인, SteamId/OwnerSteamId 파싱, VAC·퍼블리셔 밴 검사.
- 장애 격리(Polly): 서킷 오픈 → `SteamCircuitOpenException`, 타임아웃 → `SteamApiException("STEAM_API_TIMEOUT")`, 연결 실패 → `SteamApiException("FAIL_TO_CONNECT")`. 서킷브레이커 설정은 `CircuitBreakerOptions`.

> `STEAM_APP_ID`는 게임 미출시 상태라 현재 기본값 `480`(Spacewar)을 사용한다. 출시 후 실제 App ID로 교체한다.

## 세션 관리 (`SessionService` + Redis)

`ICacheStore`로 Redis에 두 개의 키를 둔다 (`CacheKey.Session`):

| 키 | 값 | 용도 |
|---|---|---|
| `session:{sessionId}` | `PlayerSession` (SteamId, TTL) | 세션 본문 — 인증 시 조회 |
| `session:steam:{steamId}` | `sessionId` | 역인덱스 — 단일 세션 강제용 |

세션 TTL은 15일이며 두 키 모두 동일 TTL로 만료된다.

### 유저당 단일 세션

- **생성 시**: 역인덱스로 기존 세션을 찾아 무효화한 뒤 새 세션을 만든다. 한 계정이 여러 곳에서 동시에 로그인 상태를 유지하지 못한다.
- **삭제 시**: 역인덱스가 *지금 지우는 세션*을 가리킬 때만 역인덱스를 정리한다. 새 로그인 직후 들어온 옛 로그아웃이 새 세션의 역인덱스를 지우는 레이스를 방지한다 (`SessionService.cs:56-59`).

## 세션 인증 (`SessionAuthorizeAttribute`)

`[SessionAuthorize]`가 붙은 엔드포인트에 적용되는 `IAsyncAuthorizationFilter`다.

1. `Session-Id` 헤더가 없으면 `401 Unauthorized`.
2. `ISessionService.GetAsync`로 세션 조회, 없으면 `401`.
3. 성공 시 `session_id` / `steam_id` 클레임을 가진 `ClaimsPrincipal`을 주입한다.

컨트롤러는 신원을 직접 파싱하지 않고 `ClaimsPrincipalExtensions`로 꺼낸다:

```csharp
var steamId = User.GetSteamId();     // steam_id 클레임 → ulong
var sessionId = User.GetSessionId(); // session_id 클레임
```

## 주요 타입 위치

| 항목 | 파일 |
|---|---|
| 컨트롤러 | `Domain/Auth/Controller/AuthController.cs` |
| 로그인 서비스 | `Domain/Auth/Service/LoginService.cs` |
| 세션 서비스 | `Domain/Auth/Service/SessionService.cs` |
| 티켓 검증 | `Global/Auth/SteamAuthTicketValidator.cs` |
| 인증 필터 | `Global/Security/SessionAuthorizeAttribute.cs` |
| 클레임 확장 | `Global/Security/ClaimsPrincipalExtensions.cs` |
| 캐시 키 | `Global/Infrastructure/Cache/CacheKey.cs` |
| 엔티티 | `Domain/Auth/Entity/User.cs`, `PlayerSession.cs` |
