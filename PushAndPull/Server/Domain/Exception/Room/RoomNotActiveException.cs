namespace Server.Domain.Exception.Room;

public class RoomNotActiveException : System.Exception
{
    public string RoomCode { get; }
    
    public RoomNotActiveException(string roomCode)
        : base($"ROOM_NOT_ACTIVE:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}