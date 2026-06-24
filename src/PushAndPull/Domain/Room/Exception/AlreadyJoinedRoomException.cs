using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Room.Exception;

public class AlreadyJoinedRoomException : BadRequestException
{
    public string RoomCode { get; }

    public AlreadyJoinedRoomException(string roomCode)
        : base($"ALREADY_JOINED:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}
