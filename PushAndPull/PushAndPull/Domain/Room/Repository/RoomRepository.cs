using Microsoft.EntityFrameworkCore;
using Npgsql;
using PushAndPull.Domain.Room.Entity;
using PushAndPull.Domain.Room.Exception;
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

    public async Task<IReadOnlyList<RoomEntity>> GetAllAsync(int skip, int take, CancellationToken ct = default)
    {
        return await _context.Rooms
            .AsNoTracking()
            .Where(x => x.Status == RoomStatus.Active && !x.IsPrivate)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(RoomEntity room, CancellationToken ct = default)
    {
        try
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            _context.Entry(room).State = EntityState.Detached;
            throw new DuplicateRoomCodeException(room.RoomCode);
        }
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
                .SetProperty(x => x.GuestSteamId, steamId)
                .SetProperty(x => x.GuestLastHeartbeatAt, DateTimeOffset.UtcNow), ct);

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
                .SetProperty(x => x.GuestSteamId, (ulong?)null)
                .SetProperty(x => x.GuestLastHeartbeatAt, (DateTimeOffset?)null), ct);

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

    public async Task<bool> UpdateHeartbeatAsync(string roomCode, ulong steamId, DateTimeOffset now, CancellationToken ct = default)
    {
        var updated = await _context.Rooms
            .Where(x => x.RoomCode == roomCode
                        && x.Status == RoomStatus.Active
                        && (x.HostSteamId == steamId || x.GuestSteamId == steamId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.LastHeartbeatAt, x => x.HostSteamId == steamId ? now : x.LastHeartbeatAt)
                .SetProperty(x => x.GuestLastHeartbeatAt, x => x.GuestSteamId == steamId ? now : x.GuestLastHeartbeatAt), ct);

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

    public async Task<int> FreeStaleGuestsAsync(DateTimeOffset cutoff, CancellationToken ct = default)
    {
        return await _context.Rooms
            .Where(x => x.Status == RoomStatus.Active
                        && x.GuestSteamId != null
                        && (x.GuestLastHeartbeatAt ?? x.LastHeartbeatAt ?? x.CreatedAt) < cutoff)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.CurrentPlayers, x => x.CurrentPlayers - 1)
                .SetProperty(x => x.GuestSteamId, (ulong?)null)
                .SetProperty(x => x.GuestLastHeartbeatAt, (DateTimeOffset?)null), ct);
    }
}
