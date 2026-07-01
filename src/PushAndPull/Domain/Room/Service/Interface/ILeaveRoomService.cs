namespace PushAndPull.Domain.Room.Service.Interface;

public interface ILeaveRoomService
{
    Task ExecuteAsync(LeaveRoomCommand request, CancellationToken ct = default);
}

public record LeaveRoomCommand(
    string RoomCode,
    ulong SteamId
    );
