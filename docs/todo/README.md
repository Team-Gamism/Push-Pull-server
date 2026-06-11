# TODO 목록

Push & Pull 서버 코드 분석(2026-06-11) 기반 작업 목록.
Steamworks P2P 협동(2인) 게임의 매치메이킹 백엔드 관점으로 정리.

> **전 항목 처리 완료 (2026-06-11, `feat/todo-backlog` 브랜치).**
> 일부 항목은 정책 결정에 따라 원문과 다르게 구현됨 — 아래 비고 참조.

## fix/ — 버그·결함

| # | 항목 | 상태 |
|---|---|---|
| [01](fix/01-return-steam-lobby-id.md) | 방 참가/조회 응답에 SteamLobbyId 반환 | ✅ 참가 응답에만 반환 (무인증 조회로 로비 ID가 새면 비밀번호 검증 우회 가능) |
| [02](fix/02-partition-rate-limiter.md) | 레이트리밋 클라이언트별 파티셔닝 | ✅ |
| [03](fix/03-validate-private-room-password.md) | 비공개 방 비밀번호 필수 검증 | ✅ 정책 변경: IsPrivate=목록 노출 여부, 비밀번호는 공개/비공개 무관 설정 가능. join 검증을 PasswordHash 유무 기준으로 전환 |
| [04](fix/04-track-room-participants.md) | 방 참가자 추적 (중복 참가 방지) | ✅ room.guest_steam_id 단일 컬럼 (2인 고정) |
| [05](fix/05-room-code-collision-retry.md) | 방 코드 충돌 시 재시도 | ✅ |
| [06](fix/06-request-input-validation.md) | 요청 DTO 입력 검증 | ✅ RoomName 50자, Nickname 32자 |
| [07](fix/07-propagate-cancellation-token.md) | CancellationToken 전파 누락 | ✅ |

## feat/ — 신규 기능

| # | 항목 | 상태 |
|---|---|---|
| [01](feat/01-leave-and-close-room.md) | 방 나가기 / 방 닫기 API | ✅ 호스트 leave 시 방 자동 Close |
| [02](feat/02-stale-room-cleanup.md) | 유령 방(stale room) 정리 | ✅ 하트비트 + BackgroundService |
| [03](feat/03-room-list-pagination.md) | 방 목록 페이지네이션 | ✅ offset 방식, size 상한 50 |
| [04](feat/04-room-response-max-players.md) | 방 조회 응답에 MaxPlayers 추가 | ✅ |
| [05](feat/05-host-migration-policy.md) | 호스트 이관 정책 결정 | ✅ 1번(자동 종료) 채택 — 별도 이관 API 없음 |

## refactor/ — 정리

| # | 항목 | 상태 |
|---|---|---|
| [01](refactor/01-remove-or-wire-dead-code.md) | 미사용 코드 정리 또는 연결 | ✅ Close 계열은 feat 01에서 사용, MarkDeleting/Deleting·User 도메인 갱신 메서드는 삭제 |
| [02](refactor/02-single-session-per-user.md) | 유저당 단일 세션 정책 | ✅ session:steam:{steamId} 역인덱스 |
| [03](refactor/03-getallroom-result-boundary.md) | GetAllRoomService 응답 DTO 경계 정리 | ✅ |
| [04](refactor/04-prune-stale-worktree.md) | 오래된 worktree 사본 정리 | ✅ prune 완료 (미병합 브랜치 claude/trusting-beaver-7e4a30 삭제는 보류) |
