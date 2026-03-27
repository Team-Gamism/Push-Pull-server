namespace PushAndPull.Domain.Room.Service.Interface;

public interface IJoinRoomService
{
    Task ExecuteAsync(JoinRoomCommand request);
}

public record JoinRoomCommand(
    string RoomCode,
    string? Password
    );
