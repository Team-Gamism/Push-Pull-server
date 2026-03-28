namespace PushAndPull.Domain.Auth.Dto.Request;

public record LoginRequest(
    string SteamTicket,
    string Nickname
    );
