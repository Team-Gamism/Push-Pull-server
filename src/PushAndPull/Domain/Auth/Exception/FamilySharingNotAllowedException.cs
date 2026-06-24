namespace PushAndPull.Domain.Auth.Exception;

public class FamilySharingNotAllowedException : SteamAuthException
{
    public FamilySharingNotAllowedException(ulong steamId)
        : base("Family sharing is not allowed", steamId)
    {
    }
}
