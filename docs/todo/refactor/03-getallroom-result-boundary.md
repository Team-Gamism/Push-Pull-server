# [Refactor] GetAllRoomService의 응답 DTO 직접 조립 제거

## 우선순위
🟢 여유 있을 때 (작은 작업)

## 현황
`GetAllRoomService`가 컨트롤러 응답 DTO인 `GetRoomResponse`를 직접 생성해
`GetAllRoomResult`에 담는다. 다른 서비스들은 Result 타입을 반환하고
컨트롤러가 응답 DTO로 변환하는데, 이 서비스만 경계가 흐려져 있다.

## 제안
- `GetAllRoomResult`가 서비스 레벨 항목 타입(예: `GetRoomResult` 재사용)을 담도록 변경
- `RoomController.GetAllRoom`에서 응답 DTO로 매핑

## 관련 파일
- `PushAndPull/Domain/Room/Service/GetAllRoomService.cs:20-29`
- `PushAndPull/Domain/Room/Dto/Response/GetAllRoomResponse.cs`
- `PushAndPull/Domain/Room/Controller/RoomController.cs:70-76`

## 완료 조건
- [ ] 서비스 계층에서 `Dto/Response` 참조 제거
- [ ] API 응답 형태는 기존과 동일 (breaking change 없음)
- [ ] `GetAllRoomServiceTests` 갱신
