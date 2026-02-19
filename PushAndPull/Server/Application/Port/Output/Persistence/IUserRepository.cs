using Server.Domain.Entity;

namespace Server.Application.Port.Output.Persistence
{
    public interface IUserRepository
    {
        Task<User?> GetBySteamIdAsync(ulong steamId, CancellationToken ct = default);
        Task CreateAsync(User user, CancellationToken ct = default);
        Task UpdateNicknameAsync(ulong steamId, string newNickname, CancellationToken ct = default);
        Task UpdateLastLoginAsync(ulong steamId, DateTime lastLoginAt, CancellationToken ct = default);
    }
}