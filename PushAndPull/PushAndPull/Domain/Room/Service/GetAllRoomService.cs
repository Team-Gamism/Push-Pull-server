using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Service;

public class GetAllRoomService : IGetAllRoomService
{
    private readonly IRoomRepository _roomRepository;

    public GetAllRoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<GetAllRoomResult> ExecuteAsync(CancellationToken ct = default)
    {
        var rooms = await _roomRepository.GetAllAsync(ct);

        var results = rooms
            .Select(room => new GetRoomResult(
                room.RoomName,
                room.RoomCode,
                room.CurrentPlayers,
                room.MaxPlayers,
                room.IsPrivate
            ))
            .ToList();

        return new GetAllRoomResult(results);
    }
}
