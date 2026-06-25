using PushAndPull.Domain.Auth.Entity;

namespace PushAndPull.Domain.Auth.Service.Interface;

public interface ISessionService
{
    Task<PlayerSession> CreateAsync(ulong steamId, TimeSpan ttl, CancellationToken ct = default);
    Task<PlayerSession?> GetAsync(string sessionId, CancellationToken ct = default);
    Task DeleteAsync(string sessionId, CancellationToken ct = default);
}
