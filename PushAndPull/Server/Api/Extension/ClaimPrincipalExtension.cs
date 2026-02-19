using System.Security.Claims;
using Server.Api.Attribute;

namespace Server.Api.Extension;

public static class ClaimPrincipalExtension
{
    public static string GetSessionId(this ClaimsPrincipal user)
    {
        return user.FindFirst(SessionClaim.SessionId)?.Value
               ?? throw new UnauthorizedAccessException("SESSION_ID_MISSING");
    }

    public static long GetSteamId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(SessionClaim.SteamId)?.Value
                    ?? throw new UnauthorizedAccessException("STEAM_ID_MISSING");

        if (!long.TryParse(value, out var steamId))
            throw new UnauthorizedAccessException("INVALID_STEAM_ID");

        return steamId;
    }
}