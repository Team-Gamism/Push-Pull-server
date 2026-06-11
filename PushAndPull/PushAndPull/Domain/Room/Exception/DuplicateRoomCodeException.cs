namespace PushAndPull.Domain.Room.Exception;

// 클라이언트 잘못이 아닌 내부 충돌 신호 — CreateRoomService의 재시도 루프에서 소비된다.
public class DuplicateRoomCodeException : System.Exception
{
    public string RoomCode { get; }

    public DuplicateRoomCodeException(string roomCode)
        : base($"DUPLICATE_ROOM_CODE:RoomCode = {roomCode}")
    {
        RoomCode = roomCode;
    }
}
