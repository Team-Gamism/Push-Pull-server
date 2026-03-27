namespace PushAndPull.Domain.Room.Service.Interface;

public interface IGetAllRoomService
{
    Task<GetAllRoomResult> ExecuteAsync(CancellationToken ct = default);
}

public record GetAllRoomResult(
    IReadOnlyList<RoomSummary> Rooms
        );
