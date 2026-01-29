using Server.Domain.Entity;

namespace Server.Application.Port.Output.Persistence;

public interface IRoomRepository
{
    Task<Room?> GetAsync(string roomCode);
    Task CreateAsync(Room room);
    Task UpdateAsync(Room room);
    Task CloseAsync(string roomCode);
}