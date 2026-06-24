namespace PushAndPull.Domain.Room.Exception;

public class RoomCodeGenerationFailedException : System.Exception
{
    public RoomCodeGenerationFailedException()
        : base("ROOM_CODE_GENERATION_FAILED")
    {
    }
}
