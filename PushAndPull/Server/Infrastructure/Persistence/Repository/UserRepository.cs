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
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateNicknameAsync(ulong steamId, string newNickname, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.SteamId == steamId, ct);
        if (user != null)
        {
            user.Nickname = newNickname;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateLastLoginAsync(ulong steamId, DateTime lastLoginAt, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.SteamId == steamId, ct);
        if (user != null)
        {
            user.LastLoginAt = lastLoginAt;
            await _context.SaveChangesAsync(ct);
        }
    }
}