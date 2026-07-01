using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace PushAndPull.Global.Infrastructure.Cache;

public class CacheStore : ICacheStore
{
    private readonly IDistributedCache _cache;

    public CacheStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);

        var options = new DistributedCacheEntryOptions();
        if (ttl.HasValue)
            options.AbsoluteExpirationRelativeToNow = ttl;

        await _cache.SetAsync(key, bytes, options, ct);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(key, ct);

        if (bytes is null)
            return default;

        return JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
    }
}
