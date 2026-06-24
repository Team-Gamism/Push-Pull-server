using PushAndPull.Domain.Room.Entity;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Service;

public class ReconnectRoomService : IReconnectRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly TimeProvider _timeProvider;

    public ReconnectRoomService(
        IRoomRepository roomRepository,
        TimeProvider timeProvider
        )
    {
        _roomRepository = roomRepository;
        _timeProvider = timeProvider;
    }

    public async Task<ReconnectRoomResult> ExecuteAsync(ReconnectRoomCommand request, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetAsync(request.RoomCode, ct)
            ?? throw new RoomNotFoundException(request.RoomCode);

        if (room.Status != RoomStatus.Active)
            throw new RoomNotActiveException(request.RoomCode);

        var isHost = room.HostSteamId == request.SteamId;
        var isGuest = room.GuestSteamId == request.SteamId;
        if (!isHost && !isGuest)
            throw new RoomNotParticipantException(request.RoomCode);

        // 유예 시계를 리셋해 곧 있을 cleanup sweep과의 레이스를 막는다.
        var now = _timeProvider.GetUtcNow();
        var success = await _roomRepository.UpdateHeartbeatAsync(request.RoomCode, request.SteamId, now, ct);
        if (!success)
        {
            // Get~Update 사이 레이스: sweep가 게스트 슬롯을 해제했거나 방이 닫혔다.
            var roomAfterAttempt = await _roomRepository.GetAsync(request.RoomCode, ct);
            if (roomAfterAttempt == null)
                throw new RoomNotFoundException(request.RoomCode);
            if (roomAfterAttempt.Status != RoomStatus.Active)
                throw new RoomNotActiveException(request.RoomCode);

            throw new RoomNotParticipantException(request.RoomCode);
        }

        return new ReconnectRoomResult(room.SteamLobbyId, isHost ? "Host" : "Guest");
    }
}
