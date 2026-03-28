using PushAndPull.Domain.Auth.Entity;

namespace PushAndPull.Domain.Auth.Repository.Interface;

public interface IUserRepository
{
    Task<User?> GetBySteamIdAsync(ulong steamId, CancellationToken ct = default);
    Task CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(ulong steamId, string nickname, DateTime lastLoginAt, CancellationToken ct = default);
}
