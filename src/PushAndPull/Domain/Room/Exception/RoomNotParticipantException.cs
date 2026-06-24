using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Room.Exception;

public class RoomNotParticipantException : BadRequestException
{
    public string RoomCode { get; }

    public RoomNotParticipantException(string roomCode)
        : base($"NOT_PARTICIPANT:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}
