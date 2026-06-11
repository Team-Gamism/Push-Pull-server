using PushAndPull.Domain.Room.Entity;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service.Interface;
using PushAndPull.Global.Service;

namespace PushAndPull.Domain.Room.Service;

public class JoinRoomService : IJoinRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IPasswordHasher _passwordHasher;

    public JoinRoomService(
        IRoomRepository roomRepository,
        IPasswordHasher passwordHasher
        )
    {
        _roomRepository = roomRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<JoinRoomResult> ExecuteAsync(JoinRoomCommand request, CancellationToken ct = default)
    {
        var room = await _roomRepository.GetAsync(request.RoomCode, ct)
            ?? throw new RoomNotFoundException(request.RoomCode);

        if (room.Status != RoomStatus.Active)
            throw new RoomNotActiveException(request.RoomCode);

        if (room.PasswordHash != null)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new PasswordRequiredException(request.RoomCode);

            if (!_passwordHasher.Verify(request.Password, room.PasswordHash))
                throw new InvalidPasswordException(request.RoomCode);
        }

        var success = await _roomRepository.IncrementPlayerCountAsync(request.RoomCode, ct);
        if (!success)
        {
            var roomAfterAttempt = await _roomRepository.GetAsync(request.RoomCode, ct);
            if (roomAfterAttempt == null)
                throw new RoomNotFoundException(request.RoomCode);
            if (roomAfterAttempt.Status != RoomStatus.Active)
                throw new RoomNotActiveException(request.RoomCode);

            throw new RoomFullException(request.RoomCode);
        }

        return new JoinRoomResult(room.SteamLobbyId);
    }
}
