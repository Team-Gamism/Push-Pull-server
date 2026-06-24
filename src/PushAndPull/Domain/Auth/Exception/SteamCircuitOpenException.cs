namespace PushAndPull.Domain.Auth.Exception;

public class SteamCircuitOpenException(System.Exception innerException)
    : SteamApiException("STEAM_API_CIRCUIT_OPEN", innerException);
