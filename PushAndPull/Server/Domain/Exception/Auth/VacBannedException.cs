namespace Server.Domain.Exception.Auth;

public class VacBannedException : SteamAuthException
{
    public VacBannedException(ulong steamId) 
        : base($"User is VAC banned", steamId)
    {
    }
}