namespace Server.Infrastructure.Auth.Dto;

public record SteamAuthResponseParams(
    string Result,
    string SteamId,
    string OwnerSteamId,
    bool VacBanned,
    bool PublisherBanned
);