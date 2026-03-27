using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Service;

public class GetRoomService : IGetRoomService
{
    private readonly IRoomRepository _roomRepository;

    public GetRoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<GetRoomResult> ExecuteAsync(GetRoomCommand request)
    {
        if (string.IsNullOrEmpty(request.RoomCode))
            throw new ArgumentException("REQUIRED_ROOMCODE");

        var room = await _roomRepository.GetAsync(request.RoomCode)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        return new GetRoomResult(
            room.RoomName,
            room.RoomCode,
            room.CurrentPlayers,
            room.IsPrivate
        );
    }
}
