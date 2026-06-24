namespace PushAndPull.Domain.Room.Service.Interface;

public interface IHeartbeatRoomService
{
    Task ExecuteAsync(HeartbeatRoomCommand request, CancellationToken ct = default);
}

public record HeartbeatRoomCommand(
    string RoomCode,
    ulong SteamId
    );
