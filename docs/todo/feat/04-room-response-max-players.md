# [Feat] 방 조회 응답에 MaxPlayers 추가

## 우선순위
🟢 여유 있을 때 (작은 작업)

## 배경
`GetRoomResponse`가 `CurrentPlayers`만 내려줘서
클라이언트가 `1/2` 형태의 인원 표시를 할 수 없다.

## 제안
`GetRoomResponse`에 `MaxPlayers` 필드 추가 (단건 조회·목록 조회 모두 반영).

## 관련 파일
- `PushAndPull/Domain/Room/Dto/Response/GetRoomResponse.cs`
- `PushAndPull/Domain/Room/Controller/RoomController.cs:62-67`
- `PushAndPull/Domain/Room/Service/GetAllRoomService.cs:21-26`

## 완료 조건
- [ ] 단건/목록 응답 모두에 `maxPlayers` 포함
- [ ] 관련 테스트 갱신
