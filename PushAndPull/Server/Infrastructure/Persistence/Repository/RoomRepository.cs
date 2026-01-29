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