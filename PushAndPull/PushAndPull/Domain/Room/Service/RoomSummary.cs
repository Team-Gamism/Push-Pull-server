namespace PushAndPull.Domain.Room.Service;

public record RoomSummary(
    string RoomName,
    string RoomCode,
    int CurrentPlayers,
    bool IsPrivate
    );
