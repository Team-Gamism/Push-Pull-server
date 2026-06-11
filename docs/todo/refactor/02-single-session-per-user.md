# [Refactor] 유저당 단일 세션 정책

## 우선순위
🟢 여유 있을 때

## 현황
로그인할 때마다 새 세션(15일 TTL)을 생성하고 기존 세션은 정리하지 않는다.
같은 유저가 로그인을 반복하면 Redis에 유효한 세션이 무한 누적되고,
탈취된 구 세션도 15일간 계속 유효하다.

## 제안
- `CacheKey.Session.BySteamId(steamId)` 역인덱스 키 추가
  (규칙: 캐시 키는 반드시 `CacheKey`에서 생성)
- 로그인 시 기존 세션이 있으면 삭제 후 새 세션 발급
- 로그아웃 시 역인덱스도 함께 정리

## 관련 파일
- `PushAndPull/Domain/Auth/Service/SessionService.cs`
- `PushAndPull/Global/Infrastructure/Cache/CacheKey.cs`
- `PushAndPull/Domain/Auth/Service/LoginService.cs:48-50`

## 완료 조건
- [ ] 재로그인 시 이전 SessionId가 무효화됨
- [ ] `LoginServiceTests` / `LogoutServiceTests` 갱신
