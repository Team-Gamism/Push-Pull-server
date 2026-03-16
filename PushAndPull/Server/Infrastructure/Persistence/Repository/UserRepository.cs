using Microsoft.EntityFrameworkCore;
using Server.Application.Port.Output.Persistence;
using Server.Domain.Entity;
using Server.Infrastructure.Persistence.DbContext;

namespace Server.Infrastructure.Persistence.Repository;

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
        await _context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO game_user."user" (steam_id, nickname, created_at, last_login_at)
            VALUES ({0}, {1}, {2}, {3})
            ON CONFLICT (steam_id) DO UPDATE
            SET nickname = EXCLUDED.nickname,
                last_login_at = EXCLUDED.last_login_at
            """,
            [user.SteamId, user.Nickname, user.CreatedAt, user.LastLoginAt],
            ct
        );
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