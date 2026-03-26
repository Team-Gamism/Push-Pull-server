using PushAndPull.Domain.Room.Repository;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Service;

public class GetAllRoomUseCase : IGetAllRoomUseCase
{
    private readonly IRoomRepository _roomRepository;

    public GetAllRoomUseCase(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<GetAllRoomUseCaseResult> ExecuteAsync(CancellationToken ct = default)
    {
        var rooms = await _roomRepository.GetAllAsync(ct);

        var summaries = rooms
            .Select(room => new RoomSummary(
                room.RoomName,
                room.RoomCode,
                room.CurrentPlayers,
                room.IsPrivate
            ))
            .ToList();

        return new GetAllRoomUseCaseResult(summaries);
    }
}
