using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Room.Exception;

public class PasswordRequiredException : BadRequestException
{
    public string RoomCode { get; }

    public PasswordRequiredException(string roomCode)
        : base($"PASSWORD_REQUIRED:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}
