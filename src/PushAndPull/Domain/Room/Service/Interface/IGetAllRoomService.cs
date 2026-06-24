namespace PushAndPull.Domain.Room.Service.Interface;

public interface IGetAllRoomService
{
    Task<GetAllRoomResult> ExecuteAsync(GetAllRoomQuery request, CancellationToken ct = default);
}

public record GetAllRoomQuery(
    int Page = 1,
    int Size = 20
    );

public record GetAllRoomResult(
    IReadOnlyList<GetRoomResult> Rooms,
    int Page,
    int Size,
    bool HasNext
);
