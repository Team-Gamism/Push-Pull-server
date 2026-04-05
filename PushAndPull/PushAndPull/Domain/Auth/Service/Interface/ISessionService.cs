using PushAndPull.Domain.Auth.Entity;

namespace PushAndPull.Domain.Auth.Service.Interface;

public interface ISessionService
{
    Task<PlayerSession> CreateAsync(ulong steamId, TimeSpan ttl);
    Task<PlayerSession?> GetAsync(string sessionId);
    Task DeleteAsync(string sessionId, CancellationToken ct = default);
}
