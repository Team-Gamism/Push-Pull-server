namespace PushAndPull.Domain.Room.Service.Interface;

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
    int CurrentPlayers,
    bool IsPrivate
    );
