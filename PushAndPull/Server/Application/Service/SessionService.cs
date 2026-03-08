using Server.Application.Cache;
using Server.Application.Port.Output;
using Server.Domain.Entity;

namespace Server.Application.Service;

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

    public async Task DeleteAsync(string sessionId)
    {
        await _cacheStore.DeleteAsync(CacheKey.Session.ById(sessionId));
    }

}