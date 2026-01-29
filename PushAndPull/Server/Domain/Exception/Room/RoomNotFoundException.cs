namespace Server.Domain.Exception.Room;

public class RoomNotFoundException : System.Exception
{
    public string RoomCode { get; }

    public RoomNotFoundException(string roomCode)
        : base($"ROOM_NOT_FOUND:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}