# TODO 목록

Push & Pull 서버 코드 분석(2026-06-11) 기반 작업 목록.
Steamworks P2P 협동(2인) 게임의 매치메이킹 백엔드 관점으로 정리.

## 우선순위 범례
- 🔴 즉시 — 기능 결함 또는 운영 장애 직결
- 🟡 다음 — 곧 필요해지는 작업
- 🟢 여유 있을 때 — 품질·정리

## fix/ — 버그·결함

| # | 항목 | 우선순위 |
|---|---|---|
| [01](fix/01-return-steam-lobby-id.md) | 방 참가/조회 응답에 SteamLobbyId 반환 | 🔴 |
| [02](fix/02-partition-rate-limiter.md) | 레이트리밋 클라이언트별 파티셔닝 | 🔴 |
| [03](fix/03-validate-private-room-password.md) | 비공개 방 비밀번호 필수 검증 | 🔴 |
| [04](fix/04-track-room-participants.md) | 방 참가자 추적 (중복 참가 방지) | 🟡 |
| [05](fix/05-room-code-collision-retry.md) | 방 코드 충돌 시 재시도 | 🟢 |
| [06](fix/06-request-input-validation.md) | 요청 DTO 입력 검증 | 🟡 |
| [07](fix/07-propagate-cancellation-token.md) | CancellationToken 전파 누락 | 🟢 |

## feat/ — 신규 기능

| # | 항목 | 우선순위 |
|---|---|---|
| [01](feat/01-leave-and-close-room.md) | 방 나가기 / 방 닫기 API | 🟡 |
| [02](feat/02-stale-room-cleanup.md) | 유령 방(stale room) 정리 | 🟡 |
| [03](feat/03-room-list-pagination.md) | 방 목록 페이지네이션 | 🟢 |
| [04](feat/04-room-response-max-players.md) | 방 조회 응답에 MaxPlayers 추가 | 🟢 |
| [05](feat/05-host-migration-policy.md) | 호스트 이관 정책 결정 | 🟢 |

## refactor/ — 정리

| # | 항목 | 우선순위 |
|---|---|---|
| [01](refactor/01-remove-or-wire-dead-code.md) | 미사용 코드 정리 또는 연결 | 🟢 |
| [02](refactor/02-single-session-per-user.md) | 유저당 단일 세션 정책 | 🟢 |
| [03](refactor/03-getallroom-result-boundary.md) | GetAllRoomService 응답 DTO 경계 정리 | 🟢 |
| [04](refactor/04-prune-stale-worktree.md) | 오래된 worktree 사본 정리 | 🟢 |

## 권장 진행 순서

1. fix 01 → 02 → 03 (즉시 항목)
2. feat 01 + fix 04 (방 나가기·참가자 추적은 한 묶음)
3. feat 02 (하트비트 + 정리 잡)
4. fix 06, 나머지 🟢 항목
