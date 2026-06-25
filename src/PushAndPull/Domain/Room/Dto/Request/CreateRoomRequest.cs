namespace PushAndPull.Domain.Room.Dto.Request;

public record CreateRoomRequest(
    ulong LobbyId,
    string RoomName,
    bool IsPrivate,
    string? Password
    );
