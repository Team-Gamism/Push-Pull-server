namespace PushAndPull.Domain.Room.Entity;

public class Room
{
    public long Id { get; set; }
    public string RoomName { get; set; }

    public string RoomCode { get; set; } = null!;
    public ulong SteamLobbyId { get; set; }

    public Auth.Entity.User Host { get; private set; }
    public ulong HostSteamId { get; set; }

    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; private set; }

    public bool IsPrivate { get; set; }
    public string? PasswordHash { get; set; }

    public RoomStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }

    private const int DefaultMaxPlayers = 2;

    protected Room() { }

    public Room(
        string roomCode,
        string roomName,
        ulong steamLobbyId,
        ulong hostSteamId,
        bool isPrivate,
        string? passwordHash
    )
    {
        RoomCode = roomCode;
        RoomName = roomName;
        SteamLobbyId = steamLobbyId;
        HostSteamId = hostSteamId;
        CurrentPlayers = 1;
        MaxPlayers = DefaultMaxPlayers;
        IsPrivate = isPrivate;
        PasswordHash = passwordHash;
        Status = RoomStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Join()
    {
        if (CurrentPlayers >= MaxPlayers)
            throw new InvalidOperationException("FULL_ROOM");

        CurrentPlayers++;
    }

    public void MarkDeleting(TimeSpan ttl)
    {
        Status = RoomStatus.Deleting;
        ExpiresAt = DateTimeOffset.UtcNow.Add(ttl);
    }

    public void Close()
    {
        Status = RoomStatus.Closed;
        ExpiresAt = DateTimeOffset.UtcNow;
    }
}
