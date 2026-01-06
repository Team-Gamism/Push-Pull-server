using Server.Application.Port.Output;
using Server.Domain.ValueObject;
using Server.Infrastructure.Cache;

namespace Server.Application.Service;

public class SessionService
{
    private readonly ICacheStore _cacheStore;
    
    public SessionService(ICacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    public async Task<PlayerSession> CreateAsync(ulong steamId, TimeSpan ttl)
    {
        var session = new PlayerSession(
            Guid.NewGuid(),
            steamId,
            DateTime.UtcNow.Add(ttl)
        );

        await _cacheStore.SetAsync(
            CacheKey.Session.ById(session.SessionId),
            session,
            ttl
        );
        
        return session;
    }
    
    public async Task<PlayerSession?> GetAsync(Guid sessionId)
    {
        return await _cacheStore.GetAsync<PlayerSession>(CacheKey.Session.ById(sessionId));
    }
    
    public async Task RemoveSessionAsync(Guid sessionId)
    {
        var session = await GetAsync(sessionId);
        if (session == null) return;

        await _cacheStore.RemoveAsync(CacheKey.Session.ById(sessionId));
    }
}