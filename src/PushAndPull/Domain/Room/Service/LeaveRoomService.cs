using PushAndPull.Domain.Room.Entity;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Service;

public class LeaveRoomService : ILeaveRoomService
{
    private readonly IRoomRepository _roomRepository;

    public LeaveRoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task ExecuteAsync(LeaveRoomCommand request, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetAsync(request.RoomCode, ct)
            ?? throw new RoomNotFoundException(request.RoomCode);

        if (room.Status != RoomStatus.Active)
            throw new RoomNotActiveException(request.RoomCode);

        // 호스트가 나가면 방을 닫는다 (호스트 이관 없음).
        if (room.HostSteamId == request.SteamId)
        {
            await _roomRepository.CloseAsync(request.RoomCode, ct);
            return;
        }

        var success = await _roomRepository.TryRemoveGuestAsync(request.RoomCode, request.SteamId, ct);
        if (!success)
        {
            var roomAfterAttempt = await _roomRepository.GetAsync(request.RoomCode, ct);
            if (roomAfterAttempt == null)
                throw new RoomNotFoundException(request.RoomCode);

            throw new RoomNotParticipantException(request.RoomCode);
        }
    }
}
