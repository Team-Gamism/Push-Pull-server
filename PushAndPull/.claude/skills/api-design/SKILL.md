---
description: REST API design guide (.NET/C# - PushAndPull)
---

# REST API Design Guide

## URL Design

- Base prefix: `api/v1/{domain}`
- Grouped by domain: `/api/v1/auth`, `/api/v1/room`
- Resource hierarchy: `/api/v1/room/{roomCode}/join`

## Endpoints

| Method | Pattern | Auth | Description |
|--------|---------|------|-------------|
| POST | `/api/v1/{domain}` | Optional | Create |
| GET | `/api/v1/{domain}/all` | Optional | List all |
| GET | `/api/v1/{domain}/{id}` | Optional | Get single |
| POST | `/api/v1/{domain}/{id}/join` | Session | Action |

## Controller

```csharp
[Route("api/v1/room")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly ICreateRoomService _createRoomService;

    public RoomController(ICreateRoomService createRoomService)
    {
        _createRoomService = createRoomService;
    }
}
```

- `[ApiController]` + `[Route("api/v1/{domain}")]` required
- Constructor injection only
- Depend only on **Service Interfaces**

## Authentication

Apply `[SessionAuthorize]` to endpoints that require session authentication:

```csharp
[SessionAuthorize]
[HttpPost]
public async Task<...> CreateRoom([FromBody] CreateRoomRequest request)
{
    var steamId = User.GetSteamId();
    var sessionId = User.GetSessionId();
}
```

- No Bearer tokens — uses `Session-Id` header
- Use `User.GetSteamId()` / `User.GetSessionId()` extension methods

## Request Binding

```csharp
[FromBody]  // POST body
[FromRoute] // path parameter
```

## Response Format

**Data response (query):**
```csharp
[HttpGet("{roomCode}")]
public async Task<GetRoomResponse> GetRoom([FromRoute] string roomCode)
{
    var result = await _getRoomService.ExecuteAsync(new GetRoomCommand(roomCode));
    return new GetRoomResponse(result.RoomCode, result.RoomName, ...);
}
```

**Created response (`CommonApiResponse`):**
```csharp
[HttpPost]
public async Task<CommonApiResponse<CreateRoomResponse>> CreateRoom(...)
{
    ...
    return CommonApiResponse.Created("Room created.", new CreateRoomResponse(result.RoomCode));
}
```

**No return value:**
```csharp
[HttpPost("logout")]
public async Task Logout() { ... }
```

## DTO

All DTOs use `record` type:

```csharp
// Request
public record CreateRoomRequest(
    long LobbyId,
    string RoomName,
    bool IsPrivate,
    string? Password
);

// Response
public record CreateRoomResponse(
    string RoomCode
);
```

## Service Call Pattern

```csharp
var result = await _service.ExecuteAsync(new XxxCommand(
    param1,
    param2
));
```

## File Locations

```
Domain/{DomainName}/Controller/{Domain}Controller.cs
Domain/{DomainName}/Dto/Request/{Action}Request.cs
Domain/{DomainName}/Dto/Response/{Action}Response.cs
```
