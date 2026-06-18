namespace PushAndPull.Domain.Room.Dto.Response;

public record ReconnectRoomResponse(
    ulong SteamLobbyId,
    string Role
    );
