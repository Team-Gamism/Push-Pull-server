using System.Net;

namespace PushAndPull.Domain.Auth.Exception;

public class FamilySharingNotAllowedException : SteamAuthException
{
    public FamilySharingNotAllowedException(ulong steamId)
        : base(HttpStatusCode.Forbidden, "Family sharing is not allowed", steamId)
    {
    }
}
