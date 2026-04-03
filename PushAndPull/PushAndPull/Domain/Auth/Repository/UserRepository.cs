using Microsoft.EntityFrameworkCore;
using Npgsql;
using PushAndPull.Domain.Auth.Entity;
using PushAndPull.Domain.Auth.Repository.Interface;
using PushAndPull.Global.Infrastructure;

namespace PushAndPull.Domain.Auth.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetBySteamIdAsync(ulong steamId, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.SteamId == steamId, ct);
    }

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        try
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            _context.Entry(user).State = EntityState.Detached;
            await UpdateAsync(user.SteamId, user.Nickname, DateTime.UtcNow, ct);
        }
    }

    public async Task UpdateAsync(ulong steamId, string nickname, DateTime lastLoginAt, CancellationToken ct = default)
    {
        await _context.Users
            .Where(u => u.SteamId == steamId)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.Nickname, nickname)
                    .SetProperty(u => u.LastLoginAt, lastLoginAt),
                ct);
    }
}
