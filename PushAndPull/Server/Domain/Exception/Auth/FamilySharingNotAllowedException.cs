namespace Server.Domain.Exception.Auth;

public class FamilySharingNotAllowedException : SteamAuthException
{
    public FamilySharingNotAllowedException(ulong steamId)
        : base("Family sharing is not allowed", steamId)
    {
    }
}
