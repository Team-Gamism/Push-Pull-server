namespace Server.Application.Port.Input;

public interface IJoinRoomUseCase
{
    Task ExecuteAsync(JoinRoomCommand request);
}

public record JoinRoomCommand(
    string RoomCode,
    string? Password
    );