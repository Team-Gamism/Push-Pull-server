namespace Server.Application.Cache;

public static class CacheKey
{
    public static class Session
    {
        public static string ById(string sessionId) => $"session:{sessionId}";
    }
}
