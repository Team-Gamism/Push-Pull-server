using Server.Domain.Entity;

namespace Server.Application.Port.Output.Persistence;

public interface IRoomRepository
{
    Task<Room?> GetAsync(string roomCode);
    Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken ct);
    Task CreateAsync(Room room);
    Task UpdateAsync(Room room);
    Task CloseAsync(string roomCode);
}