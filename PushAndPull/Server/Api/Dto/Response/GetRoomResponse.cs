namespace Server.Api.Dto.Response;

public record GetRoomResponse(
    string RoomName,
    string RoomCode,
    int MaxPlayers,
    bool IsPrivate
    );