namespace PushAndPull.Domain.Room.Service.Interface;

public interface IJoinRoomService
{
    Task<JoinRoomResult> ExecuteAsync(JoinRoomCommand request, CancellationToken ct = default);
}

public record JoinRoomCommand(
    string RoomCode,
    string? Password
    );

public record JoinRoomResult(
    ulong SteamLobbyId
    );
