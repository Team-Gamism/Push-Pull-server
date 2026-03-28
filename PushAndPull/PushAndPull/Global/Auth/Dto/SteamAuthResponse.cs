namespace PushAndPull.Global.Auth.Dto;

public record SteamAuthResponse(
    SteamAuthResponseData Response
);

public record SteamAuthResponseData(
    SteamAuthResponseParams? Params,
    SteamAuthResponseError? Error
);

public record SteamAuthResponseError(
    int ErrorCode,
    string? ErrorDesc
);

public record SteamAuthResponseParams(
    string Result,
    string SteamId,
    string OwnerSteamId,
    bool VacBanned,
    bool PublisherBanned
);
