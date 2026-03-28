namespace PushAndPull.Domain.Room.Service.Interface;

public interface IGetRoomService
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
