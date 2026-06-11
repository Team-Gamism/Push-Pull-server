using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Room.Exception;

public class NotRoomHostException : BadRequestException
{
    public string RoomCode { get; }

    public NotRoomHostException(string roomCode)
        : base($"NOT_ROOM_HOST:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}
