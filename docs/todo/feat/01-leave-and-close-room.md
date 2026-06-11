# [Feat] 방 나가기 / 방 닫기 API

## 우선순위
🟡 다음 (fix 04·05의 근본 해결)

## 배경
퇴장/종료 API가 없어 `CurrentPlayers`는 증가만 한다.
2인 방이 한 번 차면 영구적으로 가득 찬 상태가 된다.
이미 구현돼 있으나 미사용 상태인 `RoomRepository.CloseAsync`,
`Room.Close()`, `RoomStatus.Closed`가 이 기능에 들어맞는다.

## 제안 API
| Method | Route | 동작 |
|---|---|---|
| `POST` | `api/v1/room/{roomCode}/leave` | 참가자 퇴장, `CurrentPlayers` 감소 |
| `DELETE` | `api/v1/room/{roomCode}` | 호스트만 가능, 방 Close 처리 |

## 정책 결정 필요
- 호스트가 leave 하는 경우: 방 자동 Close vs 거부 (→ feat 05 호스트 이관과 연계)
- 마지막 참가자가 나가면 방 자동 Close 여부

## 관련 파일
- `PushAndPull/Domain/Room/Controller/RoomController.cs`
- `PushAndPull/Domain/Room/Repository/RoomRepository.cs:52-59` (CloseAsync 활용)
- `PushAndPull/Domain/Room/Entity/Room.cs:56-60` (Close 활용)

## 완료 조건
- [ ] `LeaveRoomService`, `CloseRoomService` 신규 작성 (`ExecuteAsync` 컨벤션)
- [ ] 호스트가 아닌 유저의 DELETE 요청 거부
- [ ] `CurrentPlayers` 감소가 0 미만으로 내려가지 않음 (조건부 `ExecuteUpdateAsync`)
- [ ] 서비스 단위 테스트 추가
