---
description: Domain layer patterns (Service, Repository, Controller). Applied when working on Domain/** files.
globs: ["PushAndPull/Domain/**"]
alwaysApply: false
---

## Service Pattern

Each use case is a single class implementing a single interface with one `ExecuteAsync` method.
`Command` (input) and `Result` (output) records are defined in the same Interface file.

```csharp
// Interface file: Domain/Room/Service/Interface/ICreateRoomService.cs
public interface ICreateRoomService
{
    Task<CreateRoomResult> ExecuteAsync(CreateRoomCommand command, CancellationToken ct = default);
}

public record CreateRoomCommand(ulong HostSteamId, long LobbyId, string RoomName, bool IsPrivate, string? Password);
public record CreateRoomResult(string RoomCode);

// Implementation: Domain/Room/Service/CreateRoomService.cs
public class CreateRoomService : ICreateRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomCodeGenerator _codeGenerator;

    public CreateRoomService(IRoomRepository roomRepository, IRoomCodeGenerator codeGenerator)
    {
        _roomRepository = roomRepository;
        _codeGenerator = codeGenerator;
    }

    public async Task<CreateRoomResult> ExecuteAsync(CreateRoomCommand command, CancellationToken ct = default)
    {
        var code = _codeGenerator.Generate();
        var room = new Room(code, command.RoomName, command.HostSteamId);
        await _roomRepository.AddAsync(room, ct);
        return new CreateRoomResult(code);
    }
}
```

## Repository Pattern

- Interface in `Repository/Interface/`, implementation in `Repository/`.
- Only repositories access `AppDbContext`.

```csharp
public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _db;

    public RoomRepository(AppDbContext db) => _db = db;

    public async Task<Room?> GetByCodeAsync(string roomCode, CancellationToken ct = default)
        => await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.RoomCode == roomCode, ct);

    public async Task AddAsync(Room room, CancellationToken ct = default)
    {
        await _db.Rooms.AddAsync(room, ct);
        await _db.SaveChangesAsync(ct);
    }
}
```

## Controller Pattern

- Inject service interfaces only.
- Map `Request` → `Command`, call service, map `Result` → `Response`.
- Apply `[SessionAuthorize]` on authenticated endpoints.
- Use `User.GetSteamId()` / `User.GetSessionId()` extension methods for claims.

```csharp
[ApiController]
[Route("api/v1/room")]
public class RoomController : ControllerBase
{
    private readonly ICreateRoomService _createRoomService;

    public RoomController(ICreateRoomService createRoomService)
    {
        _createRoomService = createRoomService;
    }

    [SessionAuthorize]
    [HttpPost]
    public async Task<CommonApiResponse<CreateRoomResponse>> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var steamId = User.GetSteamId();
        var result = await _createRoomService.ExecuteAsync(
            new CreateRoomCommand(steamId, request.LobbyId, request.RoomName, request.IsPrivate, request.Password));
        return CommonApiResponse.Created("Room created.", new CreateRoomResponse(result.RoomCode));
    }
}
```
