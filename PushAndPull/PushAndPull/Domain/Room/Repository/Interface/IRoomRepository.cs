using RoomEntity = PushAndPull.Domain.Room.Entity.Room;

namespace PushAndPull.Domain.Room.Repository.Interface;

public interface IRoomRepository
{
    Task<RoomEntity?> GetAsync(string roomCode);
    Task<IReadOnlyList<RoomEntity>> GetAllAsync(CancellationToken ct);
    Task CreateAsync(RoomEntity room);
    Task<bool> IncrementPlayerCountAsync(string roomCode);
    Task CloseAsync(string roomCode);
}
