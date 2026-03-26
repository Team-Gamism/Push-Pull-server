using PushAndPull.Domain.Room.Repository;
using PushAndPull.Domain.Room.Service.Interface;
using PushAndPull.Global.Service;

namespace PushAndPull.Domain.Room.Service;

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

        var room = new Entity.Room(
            roomCode: roomCode,
            roomName: request.RoomName,
            steamLobbyId: request.LobbyId,
            hostSteamId: request.HostSteamId,
            isPrivate: request.IsPrivate,
            passwordHash: passwordHash
        );

        await _roomRepository.CreateAsync(room);

        return new CreateRoomResult(
            room.RoomCode
        );
    }
}
