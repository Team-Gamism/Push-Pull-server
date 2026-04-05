using RoomEntity = PushAndPull.Domain.Room.Entity.Room;

namespace PushAndPull.Domain.Room.Repository.Interface;

public interface IRoomRepository
{
    Task<RoomEntity?> GetAsync(string roomCode, CancellationToken ct = default);
    Task<IReadOnlyList<RoomEntity>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(RoomEntity room, CancellationToken ct = default);
    Task<bool> IncrementPlayerCountAsync(string roomCode, CancellationToken ct = default);
    Task CloseAsync(string roomCode, CancellationToken ct = default);
}
