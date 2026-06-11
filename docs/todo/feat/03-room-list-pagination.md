# [Feat] 방 목록 페이지네이션

## 우선순위
🟢 여유 있을 때 (방 수 증가 시 병목)

## 배경
`GET api/v1/room/all`이 Active 방 전체를 무제한 반환한다.
방 수가 늘어나면 응답 크기와 DB 부하가 선형으로 증가한다.

## 제안
- `?page=1&size=20` 형태의 offset 방식 (단순) 또는 `CreatedAt` 커서 방식
- `idx_room_status_created_at` 인덱스가 이미 있어 정렬 쿼리는 준비돼 있음
- 응답에 전체 개수 또는 다음 페이지 존재 여부 포함

## 관련 파일
- `PushAndPull/Domain/Room/Repository/RoomRepository.cs:25-32`
- `PushAndPull/Domain/Room/Service/GetAllRoomService.cs`
- `PushAndPull/Domain/Room/Controller/RoomController.cs:70-76`

## 완료 조건
- [ ] size 상한 강제 (예: 최대 50)
- [ ] `GetAllRoomServiceTests` 갱신
