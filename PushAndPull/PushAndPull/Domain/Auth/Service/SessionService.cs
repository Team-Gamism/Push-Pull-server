using PushAndPull.Domain.Auth.Entity;
using PushAndPull.Domain.Auth.Service.Interface;
using PushAndPull.Global.Infrastructure.Cache;

namespace PushAndPull.Domain.Auth.Service;

public class SessionService : ISessionService
{
    private readonly ICacheStore _cacheStore;

    public SessionService(ICacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    public async Task<PlayerSession> CreateAsync(ulong steamId, TimeSpan ttl)
    {
        var session = new PlayerSession(steamId, ttl);

        await _cacheStore.SetAsync(
            CacheKey.Session.ById(session.SessionId),
            session,
            session.Ttl
        );

        return session;
    }

    public async Task<PlayerSession?> GetAsync(string sessionId)
    {
        return await _cacheStore.GetAsync<PlayerSession>(CacheKey.Session.ById(sessionId));
    }

    public async Task DeleteAsync(string sessionId, CancellationToken ct = default)
    {
        await _cacheStore.DeleteAsync(CacheKey.Session.ById(sessionId));
    }
}
