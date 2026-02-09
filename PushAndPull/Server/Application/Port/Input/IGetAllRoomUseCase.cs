using Server.Application.Dto;

namespace Server.Application.Port.Input;

public interface IGetAllRoomUseCase
{
    Task<GetAllRoomUseCaseResult> ExecuteAsync(CancellationToken ct = default);
}

public record GetAllRoomUseCaseResult(
    IReadOnlyList<RoomSummary> Rooms
        );