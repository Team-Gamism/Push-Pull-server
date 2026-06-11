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

    public async Task<PlayerSession> CreateAsync(ulong steamId, TimeSpan ttl, CancellationToken ct = default)
    {
        // 유저당 단일 세션: 역인덱스로 기존 세션을 찾아 무효화한다.
        var previousSessionId = await _cacheStore.GetAsync<string>(CacheKey.Session.BySteamId(steamId), ct);
        if (previousSessionId != null)
            await _cacheStore.DeleteAsync(CacheKey.Session.ById(previousSessionId), ct);

        var session = new PlayerSession(steamId, ttl);

        await _cacheStore.SetAsync(
            CacheKey.Session.ById(session.SessionId),
            session,
            session.Ttl,
            ct
        );

        await _cacheStore.SetAsync(
            CacheKey.Session.BySteamId(steamId),
            session.SessionId,
            session.Ttl,
            ct
        );

        return session;
    }

    public async Task<PlayerSession?> GetAsync(string sessionId, CancellationToken ct = default)
    {
        return await _cacheStore.GetAsync<PlayerSession>(CacheKey.Session.ById(sessionId), ct);
    }

    public async Task DeleteAsync(string sessionId, CancellationToken ct = default)
    {
        var session = await _cacheStore.GetAsync<PlayerSession>(CacheKey.Session.ById(sessionId), ct);

        await _cacheStore.DeleteAsync(CacheKey.Session.ById(sessionId), ct);

        if (session == null)
            return;

        // 역인덱스가 이 세션을 가리킬 때만 정리한다 (재로그인 레이스 보호).
        var indexedSessionId = await _cacheStore.GetAsync<string>(CacheKey.Session.BySteamId(session.SteamId), ct);
        if (indexedSessionId == sessionId)
            await _cacheStore.DeleteAsync(CacheKey.Session.BySteamId(session.SteamId), ct);
    }
}
