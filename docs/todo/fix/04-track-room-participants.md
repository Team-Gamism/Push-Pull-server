# [Fix] 방 참가자 추적 (중복 참가 방지)

## 우선순위
🟡 다음 (feat의 방 나가기/하트비트와 한 묶음)

## 문제
`JoinRoomCommand`에 참가자 SteamId가 없어 누가 참가했는지 기록되지 않는다.

- 같은 유저가 중복 참가해 `CurrentPlayers`를 올릴 수 있음
- 호스트가 자기 방에 다시 참가할 수 있음
- 컨트롤러는 `User.GetSteamId()`를 보유하고 있으나 서비스로 전달하지 않음

## 관련 파일
- `PushAndPull/Domain/Room/Controller/RoomController.cs:78-90`
- `PushAndPull/Domain/Room/Service/JoinRoomService.cs`
- `PushAndPull/Domain/Room/Service/Interface/IJoinRoomService.cs`

## 해결 방안
- `JoinRoomCommand`에 `SteamId` 추가, 컨트롤러에서 전달
- 참가자 기록: `room_member` 테이블 또는 Redis 세트 중 선택
  (2인 협동이므로 가벼운 구조면 충분)
- 호스트 본인/이미 참가한 유저의 join 거부

## 완료 조건
- [ ] 동일 유저 중복 참가 시 도메인 예외
- [ ] 호스트 본인 참가 시 도메인 예외
- [ ] `JoinRoomServiceTests` 갱신
