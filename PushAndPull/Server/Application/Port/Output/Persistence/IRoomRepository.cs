using Server.Domain.Entity;

namespace Server.Application.Port.Output.Persistence;

public interface IRoomRepository
{
    Task<Room?> GetAsync(string roomCode);
    Task CreateAsync(Room room);
    Task<Room> UpdateAsync(Room room);
    Task DeleteAsync(string roomCode);
}