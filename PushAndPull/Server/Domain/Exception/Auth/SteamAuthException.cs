namespace Server.Domain.Exception.Auth;

public abstract class SteamAuthException : System.Exception
{
    public ulong SteamId { get; }
    
    protected SteamAuthException(string message, ulong steamId = 0) 
        : base(message)
    {
        SteamId = steamId;
    }
    
    protected SteamAuthException(string message, System.Exception innerException, ulong steamId = 0) 
        : base(message, innerException)
    {
        SteamId = steamId;
    }
}