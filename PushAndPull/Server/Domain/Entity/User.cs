namespace Server.Domain.Entity;

public class User
{
    public ulong SteamId { get; private set; }
    public string Nickname { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastLoginAt { get; set; }

    private User() { }

    public User(ulong steamId, string nickname)
    {
        SteamId = steamId;
        Nickname = nickname;
        CreatedAt = DateTime.UtcNow;
        LastLoginAt = DateTime.UtcNow;
    }

    public void UpdateNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentException("INVALID_NICKNAME");

        Nickname = nickname;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}