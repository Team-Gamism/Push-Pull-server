using Server.Domain.Entity;

namespace Server.Application.Port.Output;

public interface ISessionService
{
    Task<PlayerSession> CreateAsync(ulong steamId, TimeSpan ttl);
    Task<PlayerSession?> GetAsync(string sessionId);
    Task DeleteAsync(string sessionId);
}