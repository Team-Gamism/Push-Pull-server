using System.Net;

namespace PushAndPull.Domain.Auth.Exception;

public class VacBannedException : SteamAuthException
{
    public VacBannedException(ulong steamId)
        : base(HttpStatusCode.Forbidden, "User is VAC banned", steamId)
    {
    }
}
