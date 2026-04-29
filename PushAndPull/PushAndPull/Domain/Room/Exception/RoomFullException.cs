using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Room.Exception;

public class RoomFullException : BadRequestException
{
    public string RoomCode { get; }

    public RoomFullException(string roomCode)
        : base($"FULL_ROOM:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}
