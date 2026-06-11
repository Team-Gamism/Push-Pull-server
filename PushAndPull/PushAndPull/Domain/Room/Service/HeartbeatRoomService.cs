using PushAndPull.Domain.Room.Entity;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Service;

public class HeartbeatRoomService : IHeartbeatRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly TimeProvider _timeProvider;

    public HeartbeatRoomService(
        IRoomRepository roomRepository,
        TimeProvider timeProvider
        )
    {
        _roomRepository = roomRepository;
        _timeProvider = timeProvider;
    }

    public async Task ExecuteAsync(HeartbeatRoomCommand request, CancellationToken ct = default)
    {
        var now = _timeProvider.GetUtcNow();

        var success = await _roomRepository.UpdateHeartbeatAsync(request.RoomCode, request.SteamId, now, ct);
        if (!success)
        {
            var room = await _roomRepository.GetAsync(request.RoomCode, ct);
            if (room == null)
                throw new RoomNotFoundException(request.RoomCode);
            if (room.HostSteamId != request.SteamId)
                throw new NotRoomHostException(request.RoomCode);

            throw new RoomNotActiveException(request.RoomCode);
        }
    }
}
