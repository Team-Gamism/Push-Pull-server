# [Feat] 호스트 이관 정책 결정 및 구현

## 우선순위
🟢 여유 있을 때 (정책 결정 선행 필요)

## 배경
Steamworks 로비는 호스트 마이그레이션을 지원한다.
서버 쪽에서 호스트가 떠났을 때의 정책이 정해져 있지 않다.

## 선택지
1. **방 자동 종료**: 호스트가 나가면 방 Close — 구현 단순, 2인 협동이면 충분할 수 있음
2. **호스트 이관**: `PATCH api/v1/room/{roomCode}/host` 로 `HostSteamId` 갱신
   — Steam 로비 마이그레이션과 동기화 필요

2인 협동 게임 특성상 **1번(자동 종료)이 단순하고 충분**할 가능성이 높다.
feat 01(방 나가기/닫기) 구현 시 함께 결정하는 것이 좋다.

## 관련 파일
- `PushAndPull/Domain/Room/Entity/Room.cs` (HostSteamId, Host 네비게이션)
- `PushAndPull/Domain/Room/Entity/Config/RoomConfig.cs:83-87` (FK Restrict)

## 완료 조건
- [ ] 정책 결정 (팀 논의)
- [ ] 결정된 정책에 따른 구현 + 테스트
