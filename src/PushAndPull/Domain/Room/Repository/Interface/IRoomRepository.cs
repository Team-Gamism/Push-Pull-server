using RoomEntity = PushAndPull.Domain.Room.Entity.Room;

namespace PushAndPull.Domain.Room.Repository.Interface;

public interface IRoomRepository
{
    Task<RoomEntity?> GetAsync(string roomCode, CancellationToken ct = default);
    Task<IReadOnlyList<RoomEntity>> GetAllAsync(int skip, int take, CancellationToken ct = default);
    Task CreateAsync(RoomEntity room, CancellationToken ct = default);
    Task<bool> TryJoinAsync(string roomCode, ulong steamId, CancellationToken ct = default);
    Task<bool> TryRemoveGuestAsync(string roomCode, ulong steamId, CancellationToken ct = default);
    Task CloseAsync(string roomCode, CancellationToken ct = default);
    Task<bool> UpdateHeartbeatAsync(string roomCode, ulong steamId, DateTimeOffset now, CancellationToken ct = default);
    Task<int> CloseStaleRoomsAsync(DateTimeOffset cutoff, CancellationToken ct = default);
    Task<int> FreeStaleGuestsAsync(DateTimeOffset cutoff, CancellationToken ct = default);
}
