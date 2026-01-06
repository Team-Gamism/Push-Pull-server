namespace Server.Domain.ValueObject;

public record PlayerSession(
    Guid SessionId,
    ulong SteamId,
    DateTime ExpiredAt
    );