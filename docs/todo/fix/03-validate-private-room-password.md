# [Fix] 비공개 방 생성 시 비밀번호 필수 검증

## 우선순위
🔴 즉시

## 문제
`CreateRoomService`가 `IsPrivate=true` + 비밀번호 없음 조합을 검증 없이 허용한다.
이렇게 생성된 방은 `PasswordHash`가 null인데, 참가 시
`_passwordHasher.Verify(request.Password, room.PasswordHash!)`에서
null 해시로 BCrypt가 예외를 던져 500 에러가 발생한다.

## 관련 파일
- `PushAndPull/Domain/Room/Service/CreateRoomService.cs:26-28`
- `PushAndPull/Domain/Room/Service/JoinRoomService.cs:36`

## 해결 방안
- 생성 시점에 `IsPrivate && string.IsNullOrWhiteSpace(Password)`면 도메인 예외 throw
  (예: `PasswordRequiredException` 재사용 또는 신규 예외)
- 반대로 `IsPrivate=false`인데 비밀번호가 온 경우의 정책(무시 vs 에러)도 함께 결정

## 완료 조건
- [ ] 비밀번호 없는 비공개 방 생성 요청이 도메인 예외로 거부됨
- [ ] `CreateRoomServiceTests`에 검증 테스트 추가
