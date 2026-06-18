namespace PushAndPull.Domain.Room.Service.Interface;

public interface IReconnectRoomService
{
    Task<ReconnectRoomResult> ExecuteAsync(ReconnectRoomCommand request, CancellationToken ct = default);
}

public record ReconnectRoomCommand(
    string RoomCode,
    ulong SteamId
    );

public record ReconnectRoomResult(
    ulong SteamLobbyId,
    string Role
    );
