namespace Server.Api.Dto.Request;

public record JoinRoomRequest(
    string RoomCode,
    string? Password
    );