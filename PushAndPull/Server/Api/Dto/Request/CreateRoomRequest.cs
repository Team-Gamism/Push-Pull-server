namespace Server.Api.Dto.Request;

public record CreateRoomRequest(
    ulong LobbyId,
    string RoomName,
    bool IsPrivate,
    string? Password,
    int MaxPlayers,
    long HostSteamId
    );