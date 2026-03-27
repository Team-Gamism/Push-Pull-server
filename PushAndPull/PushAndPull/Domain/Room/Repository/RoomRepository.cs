using Microsoft.EntityFrameworkCore;
using PushAndPull.Domain.Room.Entity;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Global.Infrastructure;
using RoomEntity = PushAndPull.Domain.Room.Entity.Room;

namespace PushAndPull.Domain.Room.Repository;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RoomEntity?> GetAsync(string roomCode)
    {
        return await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoomCode == roomCode);
    }

    public async Task<IReadOnlyList<RoomEntity>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Where(x => x.Status == RoomStatus.Active)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(RoomEntity room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IncrementPlayerCountAsync(string roomCode)
    {
        var updated = await _context.Rooms
            .Where(x => x.RoomCode == roomCode
                        && x.Status == RoomStatus.Active
                        && x.CurrentPlayers < x.MaxPlayers)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.CurrentPlayers, x => x.CurrentPlayers + 1));

        return updated > 0;
    }

    public async Task CloseAsync(string roomCode)
    {
        await _context.Rooms
            .Where(x => x.RoomCode == roomCode)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, RoomStatus.Closed)
                .SetProperty(x => x.ExpiresAt, DateTimeOffset.UtcNow));
    }
}
