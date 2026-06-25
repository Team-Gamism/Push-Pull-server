using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Room.Exception;

public class InvalidPasswordException : BadRequestException
{
    public string RoomCode { get; }

    public InvalidPasswordException(string roomCode)
        : base($"INVALID_PASSWORD:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}
