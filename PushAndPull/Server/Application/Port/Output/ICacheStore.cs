namespace Server.Application.Port.Output;

public interface ICacheStore
{
    Task SetAsync<T>(string key, T value, TimeSpan? ttl  = null);
    Task<T?> GetAsync<T>(string key);
    Task DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
}