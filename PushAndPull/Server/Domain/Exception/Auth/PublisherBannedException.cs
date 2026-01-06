namespace Server.Domain.Exception.Auth;

public class PublisherBannedException : SteamAuthException
{
    public PublisherBannedException(ulong steamId) 
        : base($"User is banned by publisher", steamId)
    {
    }
}