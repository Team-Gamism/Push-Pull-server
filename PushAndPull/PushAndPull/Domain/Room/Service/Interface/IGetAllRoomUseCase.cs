namespace PushAndPull.Domain.Room.Service.Interface;

public interface IGetAllRoomUseCase
{
    Task<GetAllRoomUseCaseResult> ExecuteAsync(CancellationToken ct = default);
}

public record GetAllRoomUseCaseResult(
    IReadOnlyList<RoomSummary> Rooms
        );
