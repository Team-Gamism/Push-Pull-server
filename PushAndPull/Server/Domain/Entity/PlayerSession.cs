namespace Server.Domain.Entity;

public class PlayerSession
{
    public string SessionId { get; private set; }
    public ulong SteamId { get; private set; }
    public TimeSpan Ttl { get; private set; }
    
    private PlayerSession() { }
    public PlayerSession(ulong steamId, TimeSpan ttl)
    {
        SessionId = Guid.NewGuid().ToString();
        SteamId = steamId;
        Ttl = ttl;
    }
}