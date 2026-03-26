namespace PushAndPull.Domain.Room.Service.Interface;

public interface IJoinRoomUseCase
{
    Task ExecuteAsync(JoinRoomCommand request);
}

public record JoinRoomCommand(
    string RoomCode,
    string? Password
    );
