namespace Server.Api.Dto.Response;

public record GetRoomResponse(
    string RoomCode,
    string RoomName,
    int CurrentPlayers,
    bool IsPrivate
    );