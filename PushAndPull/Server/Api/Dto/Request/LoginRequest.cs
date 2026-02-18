namespace Server.Api.Dto.Request;

public record LoginRequest(
    string SteamTicket,
    string Nickname
    );