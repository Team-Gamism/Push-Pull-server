namespace Server.Infrastructure.Auth.Dto;

public record SteamAuthResponse(
    SteamAuthResponseData Response
);

public record SteamAuthResponseData(
    SteamAuthResponseParams Params
);

public record SteamAuthResponseParams(
    string Result,
    string SteamId,
    string OwnerSteamId,
    bool VacBanned,
    bool PublisherBanned
);