namespace Server.Infrastructure.Cache;

public static class CacheKey
{
    public static class Session
    {
        public static string ById(Guid sessionId) => $"session:{sessionId}";
    }
}