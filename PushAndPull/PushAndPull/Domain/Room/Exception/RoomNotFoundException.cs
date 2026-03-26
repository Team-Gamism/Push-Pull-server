using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Room.Exception;

public class RoomNotFoundException : NotFoundException
{
    public string RoomCode { get; }

    public RoomNotFoundException(string roomCode)
        : base($"ROOM_NOT_FOUND:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}
