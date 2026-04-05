## API Flow Diagrams

### POST /api/v1/auth/login — Steam 로그인

```mermaid
sequenceDiagram
    Client->>AuthController: POST /api/v1/auth/login {ticket, nickname}
    AuthController->>LoginService: ExecuteAsync(LoginCommand)
    LoginService->>IAuthTicketValidator: ValidateAsync(ticket)
    IAuthTicketValidator->>SteamAPI: AuthenticateUserTicket
    SteamAPI-->>IAuthTicketValidator: steamId
    IAuthTicketValidator-->>LoginService: AuthTicketValidationResult
    LoginService->>IUserRepository: GetBySteamIdAsync(steamId)
    IUserRepository-->>LoginService: null → CreateAsync(user)
    IUserRepository-->>LoginService: user → UpdateAsync(nickname)
    LoginService->>ISessionService: CreateAsync(steamId, ttl)
    ISessionService->>Redis: SET session:{id} steamId EX
    ISessionService-->>LoginService: PlayerSession
    AuthController-->>Client: 200 {sessionId}
```

### POST /api/v1/auth/logout — 로그아웃

```mermaid
sequenceDiagram
    Client->>AuthController: POST /api/v1/auth/logout (Session-Id)
    Note over AuthController: [SessionAuthorize]
    AuthController->>LogoutService: ExecuteAsync(LogoutCommand)
    LogoutService->>ISessionService: DeleteAsync(sessionId)
    ISessionService->>Redis: DEL session:{id}
    AuthController-->>Client: 200
```

### POST /api/v1/room — 방 생성

```mermaid
sequenceDiagram
    Client->>RoomController: POST /api/v1/room {lobbyId, roomName, ...}
    Note over RoomController: [SessionAuthorize]
    RoomController->>CreateRoomService: ExecuteAsync(CreateRoomCommand)
    CreateRoomService->>IRoomCodeGenerator: Generate()
    CreateRoomService->>IRoomRepository: AddAsync(room)
    IRoomRepository->>PostgreSQL: INSERT room
    RoomController-->>Client: 201 {roomCode}
```

### GET /api/v1/room/all — 방 목록 조회

```mermaid
sequenceDiagram
    Client->>RoomController: GET /api/v1/room/all
    RoomController->>GetAllRoomService: ExecuteAsync()
    GetAllRoomService->>IRoomRepository: GetAllActiveAsync()
    IRoomRepository->>PostgreSQL: SELECT rooms (AsNoTracking)
    IRoomRepository-->>GetAllRoomService: rooms
    RoomController-->>Client: 200 [roomList]
```

### GET /api/v1/room/{roomCode} — 방 상세 조회

```mermaid
sequenceDiagram
    Client->>RoomController: GET /api/v1/room/{roomCode}
    RoomController->>GetRoomService: ExecuteAsync(GetRoomCommand)
    GetRoomService->>IRoomRepository: GetByCodeAsync(roomCode)
    IRoomRepository->>PostgreSQL: SELECT room (AsNoTracking)
    IRoomRepository-->>GetRoomService: null → NotFoundException
    IRoomRepository-->>GetRoomService: room
    RoomController-->>Client: 200 {roomDetail}
```

### POST /api/v1/room/{roomCode}/join — 방 참여

```mermaid
sequenceDiagram
    Client->>RoomController: POST /api/v1/room/{roomCode}/join
    Note over RoomController: [SessionAuthorize]
    RoomController->>JoinRoomService: ExecuteAsync(JoinRoomCommand)
    JoinRoomService->>IRoomRepository: GetByCodeAsync(roomCode)
    IRoomRepository-->>JoinRoomService: null → NotFoundException
    JoinRoomService->>Room: Join(steamId)
    JoinRoomService->>IRoomRepository: UpdateAsync(room)
    IRoomRepository->>PostgreSQL: UPDATE room
    RoomController-->>Client: 200 {lobbyId}
```
