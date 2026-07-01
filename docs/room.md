# Room (방)

방 생성·참여·생존(하트비트) 흐름을 정리한다.

## 개요

- 방은 PostgreSQL에 영속한다(세션과 달리 Redis가 아님).
- 구조는 **호스트 1 + 게스트 1 = 최대 2인**(`DefaultMaxPlayers = 2`)이다.
- 입장/퇴장 등 경쟁이 발생하는 변경은 모두 **DB의 조건부 `ExecuteUpdate`(원자적 UPDATE)** 로 처리해 동시성 문제를 막는다.
- 끊긴 방/게스트는 하트비트 + 백그라운드 sweep으로 자동 정리한다.

요청 흐름: `RoomController → *RoomService → IRoomRepository`.

## 핵심 개념: RoomCode ↔ SteamLobbyId (클라이언트 필독)

**6자리 `RoomCode`는 "사람이 공유하기 쉬운 `SteamLobbyId`의 별칭"이다.** 서버는 둘 사이를 이어주는 중개자 역할만 하고, 실제 게임 연결은 여전히 Steam P2P로 클라이언트끼리 직접 한다.

| | `SteamLobbyId` | `RoomCode`    |
|---|---|---------------|
| 형태 | `109775241012345678` (17~18자리 숫자, `ulong`) | `ABC123` (6자) |
| 용도 | Steam SDK가 실제 P2P 연결에 사용 | 사람이 외우고 친구에게 공유 |
| 노출 | 클라이언트끼리만 | 플레이어끼리 전달     |

`SteamLobbyId`는 사람이 부르기엔 너무 길다. 그래서 서버가 짧은 별칭을 발급하고, 입장할 때 별칭 → 원본으로 되돌려준다.

### 방 생성 (호스트)

```
1. [클라] Steam SDK로 로비 생성             → SteamLobbyId 획득
2. [클라] POST /api/v1/room
          { lobbyId, roomName, isPrivate, password? }   ← SteamLobbyId를 그대로 전달
3. [서버] 6자리 RoomCode 발급 + (RoomCode → SteamLobbyId) 저장
4. [서버] { roomCode: "ABC123" } 반환
5. [클라] "ABC123"을 화면에 표시 → 친구에게 공유
```

호스트는 **먼저 Steam 로비를 만든 뒤** 그 LobbyId를 들고 서버에 방을 만든다. 서버가 긴 LobbyId를 6자리 코드로 바꿔 돌려준다.

### 방 입장 (게스트)

```
1. [게스트] 호스트에게 받은 "ABC123" 입력
2. [클라]   POST /api/v1/room/ABC123/join
            { password? }                  ← 코드는 URL 경로, 비번만 본문
3. [서버]   ABC123 조회 → 검증(활성/정원/비번) → 게스트 슬롯 원자적 점유
4. [서버]   { steamLobbyId: 109775... } 반환   ← 코드로부터 복원한 SteamLobbyId
5. [클라]   받은 steamLobbyId로 Steam SDK 로비 Join → 실제 P2P 연결
```

**입장의 핵심은 4번이다.** 게스트는 `RoomCode`만 알고 `SteamLobbyId`는 모른다. 서버가 코드를 받아 저장해 둔 `SteamLobbyId`를 돌려주고, 게스트는 그걸로 실제 Steam 로비에 붙는다. (`reconnect`도 동일하게 `SteamLobbyId`를 반환한다.)

## 엔드포인트

| 메서드 | 경로 | 인증 | Rate Limit | 설명 |
|---|---|---|---|---|
| `POST` | `/api/v1/room` | `[SessionAuthorize]` | `create_room` | 방 생성 |
| `GET` | `/api/v1/room/{roomCode}` | 불필요 | — | 단건 조회 |
| `GET` | `/api/v1/room/all` | 불필요 | — | 공개·활성 방 목록(페이징) |
| `POST` | `/api/v1/room/{roomCode}/join` | `[SessionAuthorize]` | `join_room` | 방 참여 |
| `POST` | `/api/v1/room/{roomCode}/leave` | `[SessionAuthorize]` | — | 방 나가기 |
| `DELETE` | `/api/v1/room/{roomCode}` | `[SessionAuthorize]` | — | 방 닫기(호스트만) |
| `POST` | `/api/v1/room/{roomCode}/heartbeat` | `[SessionAuthorize]` | — | 생존 신호 갱신 |
| `POST` | `/api/v1/room/{roomCode}/reconnect` | `[SessionAuthorize]` | `join_room` | 재접속 |

인증 엔드포인트는 `User.GetSteamId()`로 신원을 얻는다. 응답은 모두 `CommonApiResponse`로 감싼다.

## 엔티티 (`Room`)

| 필드 | 의미 |
|---|---|
| `RoomCode` | 외부 노출용 방 코드(유니크). 클라이언트가 이 값으로 접근한다 |
| `SteamLobbyId` | 연결된 Steam 로비 ID. 참여/재접속 응답으로 반환한다 |
| `HostSteamId` / `GuestSteamId` | 호스트/게스트 SteamId. 게스트는 nullable |
| `CurrentPlayers` / `MaxPlayers` | 현재/최대 인원(기본 2) |
| `IsPrivate` | **목록 노출 여부만** 결정. `true`면 `all` 목록에서 제외 |
| `PasswordHash` | 비밀번호 해시(BCrypt). null이면 공개 입장 |
| `Status` | `Active` / `Closed` |
| `LastHeartbeatAt` / `GuestLastHeartbeatAt` | 호스트/게스트 마지막 생존 신호 |

> **`IsPrivate`와 비밀번호는 독립적이다.** `IsPrivate`는 목록 노출 여부만 정하고, 비밀번호는 공개/비공개와 무관하게 설정할 수 있다. 즉 "목록에는 보이지만 비밀번호가 필요한 방", "목록엔 없지만 비밀번호 없는 방"이 모두 가능하다.

## 조회

### 단건 `GET /{roomCode}`

`RoomNotFoundException`(404 매핑)을 제외하면 상태와 무관하게 조회 가능. 응답(`GetRoomResponse`)은 `HasPassword`(= `PasswordHash != null`)만 노출하고 해시 자체는 절대 내보내지 않는다.

### 목록 `GET /all?page=1&size=20`

- `Active && !IsPrivate`인 방만, `CreatedAt` 최신순.
- `page`는 최소 1, `size`는 1~50으로 클램프.
- **`size + 1`개를 조회**해 `HasNext`를 count 쿼리 없이 판별한다 (`GetAllRoomService.cs:22-24`).

## 생성 `POST /api/v1/room`

요청 (`CreateRoomRequest`): `LobbyId`, `RoomName`, `IsPrivate`, `Password?`.

1. 방 이름 검증 — 공백 불가, 최대 50자. 위반 시 `InvalidRoomNameException`.
2. 비밀번호가 있으면 `IPasswordHasher.Hash`로 해시(원문 저장 금지 — 보안 규칙).
3. 방 코드 생성 후 저장. **유니크 충돌(`DuplicateRoomCodeException`) 시 최대 3회 재시도**, 모두 실패하면 `RoomCodeGenerationFailedException`.

호스트는 생성과 동시에 입장 처리(`CurrentPlayers = 1`)된다. 응답은 `RoomCode`.

## 참여 `POST /{roomCode}/join`

요청 (`JoinRoomRequest`): `Password?`.

검증 → 원자적 입장 순서로 동작한다 (`JoinRoomService.cs`):

1. 방 조회(없으면 `RoomNotFoundException`), `Active` 확인(아니면 `RoomNotActiveException`).
2. 이미 호스트/게스트면 `AlreadyJoinedRoomException`.
3. 비밀번호 방이면 입력 필수(`PasswordRequiredException`), `IPasswordHasher.Verify`로 검증(`InvalidPasswordException`).
4. **`TryJoinAsync`** — 단일 UPDATE로 `Active + 정원 미만 + 게스트 슬롯 비어있음 + 호스트 ≠ 본인` 조건을 만족할 때만 게스트 슬롯을 채운다. 동시에 두 명이 들어와도 한 명만 성공한다.
5. 실패 시 현재 상태를 재조회해 정확한 사유(`RoomFull` / `NotFound` / `NotActive` / `AlreadyJoined`)로 예외를 던진다.

응답은 `SteamLobbyId`로, 클라이언트는 이 값으로 Steam 로비에 접속한다.

## 나가기 / 닫기

### 나가기 `POST /{roomCode}/leave`

- **호스트가 나가면 방 전체를 닫는다**(호스트 이관 없음, `LeaveRoomService.cs:25-30`).
- 게스트면 `TryRemoveGuestAsync`로 슬롯만 해제. 참가자가 아니면 `RoomNotParticipantException`.

### 닫기 `DELETE /{roomCode}`

호스트만 가능. 호스트가 아니면 `NotRoomHostException`. `CloseAsync`는 `Active`인 방만 `Closed`로 바꾼다.

## 생존(하트비트) & 자동 정리

방이 비정상 종료(클라이언트 강제 종료 등)로 남는 것을 막는 핵심 메커니즘이다.

### 하트비트 `POST /{roomCode}/heartbeat`

`UpdateHeartbeatAsync`가 호출자가 호스트면 `LastHeartbeatAt`, 게스트면 `GuestLastHeartbeatAt`를 갱신한다. 갱신 행이 없으면 사유를 재조회해 `RoomNotFound` / `RoomNotParticipant` / `RoomNotActive`로 구분한다.

### 백그라운드 정리 (`StaleRoomCleanupService`)

`BackgroundService` + `PeriodicTimer`로 `SweepIntervalSeconds`마다 sweep한다 (`RoomCleanupOptions`):

- **`CloseStaleRoomsAsync`** — `(LastHeartbeatAt ?? CreatedAt) < cutoff`인 활성 방을 닫는다(호스트가 끊긴 방).
- **`FreeStaleGuestsAsync`** — 게스트 하트비트만 끊긴 경우 게스트 슬롯만 해제한다.

`cutoff = now - HeartbeatTimeoutSeconds`. 테스트 가능성을 위해 `TimeProvider`를 주입받는다.

### 재접속 `POST /{roomCode}/reconnect`

방 참가자(호스트/게스트)가 일시적 단절 후 돌아올 때 사용한다. 하트비트를 즉시 갱신해 곧 실행될 sweep과의 레이스를 막고(`ReconnectRoomService.cs:35-37`), 응답으로 `SteamLobbyId`와 역할(`"Host"`/`"Guest"`)을 돌려준다.

## 동시성 설계 요약

입장/퇴장/하트비트/정리는 조회 후 메모리에서 판단하지 않고, **조건을 `WHERE`에 담은 단일 `ExecuteUpdate`** 로 처리한다. "조회 → 판단 → 저장" 사이의 레이스를 DB가 원자적으로 막아주며, 영향 행 수(`updated > 0`)로 성공 여부를 판별한 뒤 실패 시에만 재조회해 사용자 친화적 예외로 변환한다.

## 주요 타입 위치

| 항목 | 파일 |
|---|---|
| 컨트롤러 | `Domain/Room/Controller/RoomController.cs` |
| 엔티티 | `Domain/Room/Entity/Room.cs`, `RoomStatus.cs` |
| 리포지토리 | `Domain/Room/Repository/RoomRepository.cs` |
| 생성/참여/나가기 등 서비스 | `Domain/Room/Service/*.cs` |
| 자동 정리 | `Domain/Room/Service/StaleRoomCleanupService.cs` |
| 정리 옵션 | `Domain/Room/Config/RoomCleanupOptions.cs` |
| 방 코드 생성 | `Global/Service/RoomCodeGenerator.cs` |
| 비밀번호 해시 | `Global/Service/PasswordHasher.cs` |
