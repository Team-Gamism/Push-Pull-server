namespace PushAndPull.Global.Infrastructure.Cache;

public interface ICacheStore
{
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}
