using Microsoft.EntityFrameworkCore;
using Server.Application.Port.Output.Persistence;
using Server.Domain.Entity;
using Server.Infrastructure.Persistence.DbContext;

namespace Server.Infrastructure.Persistence.Repository;

public class RoomRepository : IRoomRepository
{
    private readonly RoomContext _context;
    
    public RoomRepository(RoomContext context)
    {
        _context = context;
    }
    
    public async Task<Room?> GetAsync(string roomCode)
    {
        return await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoomCode == roomCode);
    }

    public async Task CreateAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public async Task<Room> UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task DeleteAsync(string roomCode)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(x => x.RoomCode == roomCode);

        if (room == null)
            return;

        room.Status = "CLOSED";
        room.ExpiresAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}