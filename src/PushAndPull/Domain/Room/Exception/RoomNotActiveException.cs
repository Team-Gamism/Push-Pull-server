using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Room.Exception;

public class RoomNotActiveException : BadRequestException
{
    public string RoomCode { get; }

    public RoomNotActiveException(string roomCode)
        : base($"ROOM_NOT_ACTIVE:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}
