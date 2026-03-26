using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Service;

public class GetRoomUseCase : IGetRoomUseCase
{
    private readonly IRoomRepository _roomRepository;

    public GetRoomUseCase(IRoomRepository roomRepository)
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
