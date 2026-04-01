using PushAndPull.Domain.Room.Dto.Response;
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

        var responses = rooms
            .Select(room => new GetRoomResponse(
                room.RoomCode,
                room.RoomName,
                room.CurrentPlayers,
                room.IsPrivate
            ))
            .ToList();

        return new GetAllRoomResult(responses);
    }
}
