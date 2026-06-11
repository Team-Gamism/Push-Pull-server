using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Service;

public class GetAllRoomService : IGetAllRoomService
{
    private readonly IRoomRepository _roomRepository;

    private const int MaxPageSize = 50;

    public GetAllRoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<GetAllRoomResult> ExecuteAsync(GetAllRoomQuery request, CancellationToken ct = default)
    {
        var page = Math.Max(request.Page, 1);
        var size = Math.Clamp(request.Size, 1, MaxPageSize);

        // size + 1개를 조회해 다음 페이지 존재 여부를 count 쿼리 없이 판별한다.
        var rooms = await _roomRepository.GetAllAsync((page - 1) * size, size + 1, ct);
        var hasNext = rooms.Count > size;

        var results = rooms
            .Take(size)
            .Select(room => new GetRoomResult(
                room.RoomName,
                room.RoomCode,
                room.CurrentPlayers,
                room.MaxPlayers,
                room.IsPrivate,
                room.PasswordHash != null
            ))
            .ToList();

        return new GetAllRoomResult(results, page, size, hasNext);
    }
}
