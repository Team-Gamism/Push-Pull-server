using Microsoft.EntityFrameworkCore;
using Server.Application.Port.Output.Persistence;
using Server.Domain.Entity;
using Server.Infrastructure.Persistence.DbContext;
using AppContext = Server.Infrastructure.Persistence.DbContext.AppContext;

namespace Server.Infrastructure.Persistence.Repository;

public class RoomRepository : IRoomRepository
{
    private readonly AppContext _context;
    
    public RoomRepository(AppContext context)
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

    public async Task UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task CloseAsync(string roomCode)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(x => x.RoomCode == roomCode);

        if (room == null)
            return;

        room.Close();
        await _context.SaveChangesAsync();
    }
}