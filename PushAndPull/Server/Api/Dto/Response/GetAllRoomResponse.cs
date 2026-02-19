namespace Server.Api.Dto.Response;

public record GetAllRoomResponse(
    IReadOnlyList<GetRoomResponse> Rooms
    );