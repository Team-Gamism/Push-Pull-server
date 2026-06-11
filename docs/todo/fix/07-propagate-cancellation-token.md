# [Fix] CancellationToken 전파 누락

## 우선순위
🟢 여유 있을 때

## 문제
프로젝트 fail-safe 규칙("I/O 경로에서 cancellation token 제거 금지")과 어긋나는 지점들:

- `ISessionService.CreateAsync` / `GetAsync`: `ct` 파라미터 자체가 없음
- `SessionService.DeleteAsync`: `ct`를 받지만 `ICacheStore.DeleteAsync`로 전달하지 않음
- `ICacheStore` 전 메서드: `ct` 없음
- `IAuthTicketValidator.ValidateAsync`: HTTP 호출인데 `ct` 없음

## 관련 파일
- `PushAndPull/Domain/Auth/Service/SessionService.cs`
- `PushAndPull/Domain/Auth/Service/Interface/ISessionService.cs`
- `PushAndPull/Global/Infrastructure/Cache/ICacheStore.cs`
- `PushAndPull/Global/Infrastructure/Cache/CacheStore.cs`
- `PushAndPull/Global/Auth/IAuthTicketValidator.cs`
- `PushAndPull/Global/Auth/SteamAuthTicketValidator.cs`

## 해결 방안
`ICacheStore` → `ISessionService` → 호출부 순으로 `ct = default` 추가 및 전파.
`IAuthTicketValidator.ValidateAsync(string ticket, CancellationToken ct = default)`로 변경.

## 완료 조건
- [ ] 모든 I/O 경로에서 `ct`가 끝까지 전파됨
- [ ] 기존 테스트 전체 통과
