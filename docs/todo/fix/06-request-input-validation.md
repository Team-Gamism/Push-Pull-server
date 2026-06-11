# [Fix] 요청 DTO 입력 검증 추가

## 우선순위
🟡 다음

## 문제
`LoginRequest`, `CreateRoomRequest`에 길이/형식 검증이 없다.

- `RoomName`: 길이 제한 없음 (DB 컬럼도 무제한 — `RoomConfig`에 `HasMaxLength` 없음)
- `Nickname`: 공백 검증만 있고 길이 제한 없음
- 대용량 문자열이 그대로 DB에 저장될 수 있음

## 관련 파일
- `PushAndPull/Domain/Room/Dto/Request/CreateRoomRequest.cs`
- `PushAndPull/Domain/Auth/Dto/Request/LoginRequest.cs`
- `PushAndPull/Domain/Room/Entity/Config/RoomConfig.cs:17-19`
- `PushAndPull/Domain/Auth/Entity/Config/UserConfig.cs`

## 해결 방안
- DTO에 DataAnnotations(`[MaxLength]`, `[Required]`) 또는 서비스 진입부 검증 추가
- `RoomName`, `Nickname` DB 컬럼에 `HasMaxLength` 지정 + 마이그레이션 추가
- 길이 정책 결정 필요 (예: RoomName 50자, Nickname 32자)

## 완료 조건
- [ ] 한도 초과 입력이 400/도메인 예외로 거부됨
- [ ] 새 마이그레이션 생성 및 검토
