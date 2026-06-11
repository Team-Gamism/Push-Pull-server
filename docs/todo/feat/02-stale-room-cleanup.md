# [Feat] 유령 방(stale room) 정리

## 우선순위
🟡 다음 (P2P 매치메이킹 서버에서 사실상 필수)

## 배경
호스트가 비정상 종료(강제 종료, 네트워크 단절)하면 방이 Active 상태로 영원히 남는다.
P2P 구조라 서버는 Steam 로비의 생존 여부를 알 수 없다.

## 제안
### 1단계: 호스트 하트비트
- `POST api/v1/room/{roomCode}/heartbeat` — 호스트가 주기적으로 호출 (예: 30초)
- 마지막 하트비트 시각을 Room에 기록 (`LastHeartbeatAt` 컬럼 추가 → 마이그레이션)

### 2단계: 백그라운드 정리 잡
- `BackgroundService`로 주기 실행 (예: 1분)
- 하트비트가 일정 시간(예: 2분) 끊긴 Active 방을 Close 처리
- `ExpiresAt`이 지난 방 정리 — 미사용 상태인 `Room.MarkDeleting(ttl)`,
  `RoomStatus.Deleting`, `idx_room_expires_at` 인덱스를 여기서 활용

## 관련 파일
- `PushAndPull/Domain/Room/Entity/Room.cs:50-54` (MarkDeleting 활용)
- `PushAndPull/Domain/Room/Entity/Config/RoomConfig.cs:80-81` (expires_at 인덱스)
- `PushAndPull/Program.cs` (BackgroundService 등록)

## 완료 조건
- [ ] 하트비트 엔드포인트 + 호스트 검증
- [ ] 정리 잡이 set-based 쿼리(`ExecuteUpdateAsync`)로 만료 방 일괄 Close
- [ ] 주기/타임아웃 값은 `IConfiguration`으로 주입
- [ ] 단위 테스트 (FakeTimeProvider 활용 — SteamCircuitBreakerTests 패턴 참고)
