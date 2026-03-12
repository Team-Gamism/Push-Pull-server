using Microsoft.EntityFrameworkCore;
using Server.Application.Port.Output.Persistence;
using Server.Domain.Entity;
using Server.Infrastructure.Persistence.DbContext;

namespace Server.Infrastructure.Persistence.Repository;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;
    
    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Room?> GetAsync(string roomCode)
    {
        return await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoomCode == roomCode);
    }

    public async Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Where(x => x.Status == RoomStatus.Active)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(Room room)
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

    // 주의: 이 메서드는 Room.Close()의 로직(Status = Closed, ExpiresAt = UtcNow)을 직접 반영하고 있습니다.
    // Room.Close()에 새로운 비즈니스 로직이 추가될 경우 이 메서드도 함께 수정해야 합니다.
    public async Task CloseAsync(string roomCode)
    {
        await _context.Rooms
            .Where(x => x.RoomCode == roomCode)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, RoomStatus.Closed)
                .SetProperty(x => x.ExpiresAt, DateTimeOffset.UtcNow));
    }
}