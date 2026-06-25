namespace PushAndPull.Domain.Auth.Exception;

public class VacBannedException : SteamAuthException
{
    public VacBannedException(ulong steamId)
        : base($"User is VAC banned", steamId)
    {
    }
}
