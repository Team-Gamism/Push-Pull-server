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

    public async Task ExecuteAsync(JoinRoomCommand request)
    {
        var room = await _roomRepository.GetAsync(request.RoomCode)
            ?? throw new RoomNotFoundException(request.RoomCode);

        if (room.Status != RoomStatus.Active)
            throw new RoomNotActiveException(request.RoomCode);

        if (request.Password != null)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new InvalidOperationException("PASSWORD_REQUIRED");

            if (!_passwordHasher.Verify( request.Password, room.PasswordHash!))
                throw new InvalidOperationException("INVALID_PASSWORD");
        }

        room.Join();

        var success = await _roomRepository.IncrementPlayerCountAsync(request.RoomCode);
        if (!success)
        {
            var roomAfterAttempt = await _roomRepository.GetAsync(request.RoomCode);
            if (roomAfterAttempt == null)
                throw new RoomNotFoundException(request.RoomCode);
            if (roomAfterAttempt.Status != RoomStatus.Active)
                throw new RoomNotActiveException(request.RoomCode);

            throw new InvalidOperationException("FULL_ROOM");
        }
    }
}
