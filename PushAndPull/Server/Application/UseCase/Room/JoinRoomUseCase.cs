using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.Port.Output.Persistence;
using Server.Domain.Entity;
using Server.Domain.Exception.Room;

namespace Server.Application.UseCase.Room;

public class JoinRoomUseCase : IJoinRoomUseCase
{
    private readonly IRoomRepository _roomRepository;
    private readonly IPasswordHasher _passwordHasher;

    public JoinRoomUseCase(
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

        room.Join(); // 도메인 선행 검증 (FULL_ROOM)

        var success = await _roomRepository.IncrementPlayerCountAsync(request.RoomCode);
        if (!success)
            throw new InvalidOperationException("FULL_ROOM");
    }
}