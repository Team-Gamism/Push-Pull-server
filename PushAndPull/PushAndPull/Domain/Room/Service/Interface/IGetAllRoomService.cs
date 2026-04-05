namespace PushAndPull.Domain.Room.Service.Interface;

public interface IGetAllRoomService
{
    Task<GetAllRoomResult> ExecuteAsync(CancellationToken ct = default);
}

public record GetAllRoomResult(IReadOnlyList<RoomInfo> Rooms);

public record RoomInfo(
    string RoomCode,
    string RoomName,
    int CurrentPlayers,
    bool IsPrivate
);
