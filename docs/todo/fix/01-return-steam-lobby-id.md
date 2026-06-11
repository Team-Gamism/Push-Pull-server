# [Fix] 방 참가/조회 응답에 SteamLobbyId 반환

## 우선순위
🔴 즉시 (기능이 사실상 미완성)

## 문제
`Room.SteamLobbyId`를 DB에 저장만 하고 클라이언트에 반환하는 곳이 없다.

- `POST api/v1/room/{roomCode}/join` → 빈 `CommonApiResponse` 반환
- `GET api/v1/room/{roomCode}` → `GetRoomResponse`에 `SteamLobbyId` 없음

Steamworks P2P 구조에서는 방 참가 성공 후 클라이언트가 Steam 로비에 접속해야 하는데,
**어느 로비에 접속해야 하는지 알 방법이 없다.**

## 관련 파일
- `PushAndPull/Domain/Room/Controller/RoomController.cs:81-90`
- `PushAndPull/Domain/Room/Dto/Response/GetRoomResponse.cs`
- `PushAndPull/Domain/Room/Service/JoinRoomService.cs`

## 해결 방안
- `JoinRoom` 응답에 `SteamLobbyId`를 포함하는 `JoinRoomResponse` 추가
- `IJoinRoomService.ExecuteAsync`가 `JoinRoomResult(ulong SteamLobbyId)`를 반환하도록 변경

## 완료 조건
- [ ] 방 참가 성공 시 응답 body에 `steamLobbyId` 포함
- [ ] `JoinRoomServiceTests`에 로비 ID 반환 검증 테스트 추가
