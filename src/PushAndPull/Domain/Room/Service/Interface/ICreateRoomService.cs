namespace PushAndPull.Domain.Room.Service.Interface;

public interface ICreateRoomService
{
    Task<CreateRoomResult> ExecuteAsync(CreateRoomCommand request, CancellationToken ct = default);
}

public record CreateRoomCommand(
    ulong LobbyId,
    string RoomName,
    bool IsPrivate,
    string? Password,
    ulong HostSteamId
    );

public record CreateRoomResult(
    string RoomCode
    );
