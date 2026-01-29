using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.Port.Output.Persistence;

namespace Server.Application.UseCase.Room;

public class CreateRoomUseCase : ICreateRoomUseCase
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomCodeGenerator _roomCodeGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public CreateRoomUseCase(
        IRoomRepository roomRepository,
        IRoomCodeGenerator roomCodeGenerator,
        IPasswordHasher passwordHasher
    )
    {
        _roomRepository = roomRepository;
        _roomCodeGenerator = roomCodeGenerator;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<CreateRoomResult> ExecuteAsync(CreateRoomCommand request)
    {
        string? passwordHash = null;
        if (!string.IsNullOrWhiteSpace(request.Password))
            passwordHash = _passwordHasher.Hash(request.Password);

        var roomCode = _roomCodeGenerator.Generate();
        
        var room = new Domain.Entity.Room(
            roomCode: roomCode,
            roomName: request.RoomName,
            steamLobbyId: request.LobbyId,
            hostSteamId: request.HostSteamId,
            maxPlayers: request.MaxPlayers,
            isPrivate: request.IsPrivate,
            passwordHash: passwordHash
        );
        
        await _roomRepository.CreateAsync(room);
        
        return new CreateRoomResult(
            room.RoomCode
        );
    }
}