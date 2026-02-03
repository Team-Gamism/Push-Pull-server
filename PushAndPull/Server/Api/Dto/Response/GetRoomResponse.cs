namespace Server.Api.Dto.Response;

public record GetRoomResponse(
    string RoomCode,
    string RoomName,
    int CurrnetPlayers,
    bool IsPrivate
    );