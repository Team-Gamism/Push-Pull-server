namespace PushAndPull.Domain.Room.Dto.Response;

public record GetRoomResponse(
    string RoomCode,
    string RoomName,
    int CurrentPlayers,
    bool IsPrivate
    );
