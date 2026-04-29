namespace PushAndPull.Domain.Auth.Entity;

public class User
{
    public ulong SteamId { get; private set; }
    public string Nickname { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastLoginAt { get; private set; }

    private User() { }

    public User(ulong steamId, string nickname)
    {
        SteamId = steamId;
        Nickname = nickname;
        CreatedAt = DateTimeOffset.UtcNow;
        LastLoginAt = DateTimeOffset.UtcNow;
    }

    public void UpdateNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentException("INVALID_NICKNAME");

        Nickname = nickname;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }
}
