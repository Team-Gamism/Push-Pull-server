# [Fix] 방 코드 충돌 시 재시도

## 우선순위
🟢 여유 있을 때

## 문제
`room_code`에 유니크 인덱스(`idx_room_room_code`)가 있는데,
`RoomCodeGenerator`가 생성한 코드가 충돌하면 `CreateAsync`에서
`DbUpdateException`이 발생해 그대로 500으로 터진다.
32^6 ≈ 10억 조합이라 확률은 낮지만 방어가 없다.

## 관련 파일
- `PushAndPull/Domain/Room/Service/CreateRoomService.cs:30-41`
- `PushAndPull/Domain/Room/Repository/RoomRepository.cs:34-38`

## 해결 방안
유니크 제약 위반(`DbUpdateException` → PostgreSQL 23505) 시
새 코드로 1~2회 재시도. 재시도 소진 시 도메인 예외.

## 완료 조건
- [ ] 코드 충돌 시 재시도 후 정상 생성
- [ ] `CreateRoomServiceTests`에 충돌 재시도 테스트 추가
