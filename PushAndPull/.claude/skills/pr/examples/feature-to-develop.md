# Example: Feature Branch PR (feature → develop)

## Branch context

- Current branch: `feat/join-room-api`
- Base branch: `develop`

## Suggested PR titles (3 options)

1. `feature: 방 참여 API 추가`
2. `feature: Room 도메인 참여 엔드포인트 구현`
3. `feature: 방 참여 서비스·컨트롤러 추가`

## Completed PR body example

---

## 📚작업 내용

- JoinRoomService 구현 — 방 코드 조회 및 참여자 추가
- RoomController에 `POST /api/v1/room/{roomCode}/join` 엔드포인트 추가
- JoinRoomServiceTests 추가 — 정상 참여 및 예외 케이스 검증
- RoomServiceConfig에 JoinRoomService DI 등록

## ◀️참고 사항

비공개 방 참여 시 비밀번호 검증은 `IPasswordHasher`를 통해 처리됩니다.
방이 가득 찬 경우 `ConflictException`을 반환합니다.

## ✅체크리스트

> `[ ]`안에 x를 작성하면 체크박스를 체크할 수 있습니다.

- [x] 현재 의도하고자 하는 기능이 정상적으로 작동하나요?
- [x] 변경한 기능이 다른 기능을 깨뜨리지 않나요?


> *추후 필요한 체크리스트는 업데이트 될 예정입니다.*

---

## Writing rules

- **작업 내용 bullets**: group by meaningful change, not by raw commit
- **참고 사항**: configuration notes, before/after comparisons, etc. Use `"."` if nothing to add
- Keep the total body under 2500 characters
- All text content in Korean (keep section header emojis as-is)
- No emojis in body text — section headers only
