using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Service;

public class CloseRoomService : ICloseRoomService
{
    private readonly IRoomRepository _roomRepository;

    public CloseRoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task ExecuteAsync(CloseRoomCommand request, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetAsync(request.RoomCode, ct)
            ?? throw new RoomNotFoundException(request.RoomCode);

        if (room.HostSteamId != request.SteamId)
            throw new NotRoomHostException(request.RoomCode);

        await _roomRepository.CloseAsync(request.RoomCode, ct);
    }
}
