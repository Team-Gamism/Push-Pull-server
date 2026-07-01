namespace PushAndPull.Domain.Room.Dto.Request;

public record JoinRoomRequest(
    string RoomCode,
    string? Password
    );
