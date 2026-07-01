using Gamism.SDK.Extensions.AspNetCore.Exceptions;

namespace PushAndPull.Domain.Room.Exception;

public class InvalidRoomNameException : BadRequestException
{
    public InvalidRoomNameException()
        : base("INVALID_ROOM_NAME")
    {
    }
}
