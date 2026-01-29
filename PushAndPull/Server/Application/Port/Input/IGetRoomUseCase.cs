namespace Server.Application.Port.Input;

public interface IGetRoomUseCase
{
    Task<GetRoomResult> ExecuteAsync(GetRoomCommand request);
}

public record GetRoomCommand(
    string RoomCode
    );
    
public record GetRoomResult(
    string RoomName,
    string RoomCode,
    int MaxPlayers,
    bool IsPrivate
    );