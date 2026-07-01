using System.Security.Claims;

namespace PushAndPull.Global.Security;

public static class ClaimsPrincipalExtensions
{
    public static string GetSessionId(this ClaimsPrincipal user)
    {
        return user.FindFirst(SessionClaim.SessionId)?.Value
               ?? throw new UnauthorizedAccessException("SESSION_ID_MISSING");
    }

    public static ulong GetSteamId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(SessionClaim.SteamId)?.Value
                    ?? throw new UnauthorizedAccessException("STEAM_ID_MISSING");

        if (!ulong.TryParse(value, out var steamId))
            throw new UnauthorizedAccessException("INVALID_STEAM_ID");

        return steamId;
    }
}
