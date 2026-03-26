namespace PushAndPull.Domain.Room.Service.Interface;

public interface ICreateRoomUseCase
{
    Task<CreateRoomResult> ExecuteAsync(CreateRoomCommand request);
}

public record CreateRoomCommand(
    ulong LobbyId,
    string RoomName,
    bool IsPrivate,
    string? Password,
    ulong HostSteamId
    );

public record CreateRoomResult(
    string RoomCode
    );
