using System.Net;

namespace PushAndPull.Domain.Auth.Exception;

public class PublisherBannedException : SteamAuthException
{
    public PublisherBannedException(ulong steamId)
        : base(HttpStatusCode.Forbidden, "User is banned by publisher", steamId)
    {
    }
}
