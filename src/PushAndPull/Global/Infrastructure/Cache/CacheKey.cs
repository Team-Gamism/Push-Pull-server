namespace PushAndPull.Global.Infrastructure.Cache;

public static class CacheKey
{
    public static class Session
    {
        public static string ById(string sessionId) => $"session:{sessionId}";
        public static string BySteamId(ulong steamId) => $"session:steam:{steamId}";
    }
}
