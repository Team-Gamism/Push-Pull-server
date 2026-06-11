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

    public async Task<RoomEntity?> GetAsync(string roomCode, CancellationToken ct = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RoomCode == roomCode, ct);
    }

    public async Task<IReadOnlyList<RoomEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Where(x => x.Status == RoomStatus.Active && !x.IsPrivate)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(RoomEntity room, CancellationToken ct = default)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> TryJoinAsync(string roomCode, ulong steamId, CancellationToken ct = default)
    {
        var updated = await _context.Rooms
            .Where(x => x.RoomCode == roomCode
                        && x.Status == RoomStatus.Active
                        && x.CurrentPlayers < x.MaxPlayers
                        && x.GuestSteamId == null
                        && x.HostSteamId != steamId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.CurrentPlayers, x => x.CurrentPlayers + 1)
                .SetProperty(x => x.GuestSteamId, steamId), ct);

        return updated > 0;
    }

    public async Task<bool> TryRemoveGuestAsync(string roomCode, ulong steamId, CancellationToken ct = default)
    {
        var updated = await _context.Rooms
            .Where(x => x.RoomCode == roomCode
                        && x.GuestSteamId == steamId
                        && x.CurrentPlayers > 1)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.CurrentPlayers, x => x.CurrentPlayers - 1)
                .SetProperty(x => x.GuestSteamId, (ulong?)null), ct);

        return updated > 0;
    }

    public async Task CloseAsync(string roomCode, CancellationToken ct = default)
    {
        await _context.Rooms
            .Where(x => x.RoomCode == roomCode && x.Status == RoomStatus.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, RoomStatus.Closed)
                .SetProperty(x => x.ExpiresAt, DateTimeOffset.UtcNow), ct);
    }

    public async Task<bool> UpdateHeartbeatAsync(string roomCode, ulong hostSteamId, DateTimeOffset now, CancellationToken ct = default)
    {
        var updated = await _context.Rooms
            .Where(x => x.RoomCode == roomCode
                        && x.HostSteamId == hostSteamId
                        && x.Status == RoomStatus.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.LastHeartbeatAt, now), ct);

        return updated > 0;
    }

    public async Task<int> CloseStaleRoomsAsync(DateTimeOffset cutoff, CancellationToken ct = default)
    {
        return await _context.Rooms
            .Where(x => x.Status == RoomStatus.Active
                        && (x.LastHeartbeatAt ?? x.CreatedAt) < cutoff)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, RoomStatus.Closed)
                .SetProperty(x => x.ExpiresAt, DateTimeOffset.UtcNow), ct);
    }
}
