namespace PushAndPull.Global.Auth;

public interface IAuthTicketValidator
{
    Task<AuthTicketValidationResult> ValidateAsync(string ticket, CancellationToken ct = default);
}

public record AuthTicketValidationResult(
    ulong SteamId,
    ulong OwnerSteamId,
    bool VacBanned,
    bool PublisherBanned
)
{
    public bool IsFamilySharing => SteamId != OwnerSteamId;
}
