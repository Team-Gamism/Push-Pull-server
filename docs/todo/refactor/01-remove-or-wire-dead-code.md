# [Refactor] 미사용 코드 정리 또는 연결

## 우선순위
🟢 여유 있을 때 (feat 01·02 진행 여부에 따라 방향 결정)

## 현황
호출하는 곳이 없는 코드:

| 코드 | 위치 | 비고 |
|---|---|---|
| `RoomRepository.CloseAsync` | `RoomRepository.cs:52` | feat 01(방 닫기)에서 활용 가능 |
| `Room.MarkDeleting` / `Room.Close` | `Room.cs:50-60` | feat 01·02에서 활용 가능 |
| `RoomStatus.Deleting` / `Closed` | `RoomStatus.cs` | 위와 동일 |
| `idx_room_expires_at` 인덱스 | `RoomConfig.cs:80-81` | feat 02(정리 잡)에서 활용 가능 |
| `User.UpdateNickname` / `UpdateLastLogin` | `User.cs:23-34` | `LoginService`가 `ExecuteUpdateAsync`로 우회 |

## 방향
- feat 01·02를 진행하면 Room 관련 코드는 그대로 살린다.
- `User` 도메인 메서드는 둘 중 택일:
  - `LoginService`가 엔티티를 추적 로드해서 도메인 메서드 경로로 갱신
  - set-based 갱신을 유지하고 미사용 메서드 삭제

## 완료 조건
- [ ] 미사용 코드가 모두 호출되거나 제거됨
- [ ] 빌드 및 기존 테스트 통과
