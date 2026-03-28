namespace PushAndPull.Domain.Room.Dto.Response;

public record GetAllRoomResponse(
    IReadOnlyList<GetRoomResponse> Rooms
    );
