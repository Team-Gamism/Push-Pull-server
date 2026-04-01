using PushAndPull.Domain.Room.Dto.Response;

namespace PushAndPull.Domain.Room.Service.Interface;

public interface IGetAllRoomService
{
    Task<GetAllRoomResult> ExecuteAsync(CancellationToken ct = default);
}

public record GetAllRoomResult(
    IReadOnlyList<GetRoomResponse> Rooms
);
