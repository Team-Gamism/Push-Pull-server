namespace Server.Domain.Entity;

public class Room
{
    public long Id { get; set; }
    public string Name { get; set; }
    
    public string RoomCode { get; set; } = null!;
    public ulong SteamLobbyId { get; set; }
    
    public long HostSteamId { get; set; }
    public int MaxPlayers { get; set; }
    
    public bool IsPrivate { get; set; }
    public string? PasswordHash { get; set; }
    
    public string Status { get; set; } = "ACTIVE";
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    
    protected Room() { }
    
    public Room(
        string roomCode,
        string roomName,
        ulong steamLobbyId,
        long hostSteamId,
        int maxPlayers,
        bool isPrivate,
        string? passwordHash
    )
    {
        RoomCode = roomCode;
        Name = roomName;
        SteamLobbyId = steamLobbyId;
        HostSteamId = hostSteamId;
        MaxPlayers = maxPlayers;
        IsPrivate = isPrivate;
        PasswordHash = passwordHash;
        Status = "ACTIVE";
        CreatedAt = DateTimeOffset.UtcNow;
    }
    
    public void MarkDeleting(TimeSpan ttl)
    {
        Status = "DELETING";
        ExpiresAt = DateTimeOffset.UtcNow.Add(ttl);
    }

    public void Close()
    {
        Status = "CLOSED";
        ExpiresAt = DateTimeOffset.UtcNow;
    }
}