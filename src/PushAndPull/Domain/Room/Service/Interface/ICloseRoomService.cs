namespace PushAndPull.Domain.Room.Service.Interface;

public interface ICloseRoomService
{
    Task ExecuteAsync(CloseRoomCommand request, CancellationToken ct = default);
}

public record CloseRoomCommand(
    string RoomCode,
    ulong SteamId
    );
