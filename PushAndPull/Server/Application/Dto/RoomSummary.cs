namespace Server.Application.Dto;

public record RoomSummary(
    string RoomName,
    string RoomCode,
    int CurrentPlayers,
    bool IsPrivate
    );