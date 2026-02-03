namespace Server.Domain.Entity;

public class Room
{
    public long Id { get; set; }
    public string RoomName { get; set; }
    
    public string RoomCode { get; set; } = null!;
    public ulong SteamLobbyId { get; set; }
    
    public long HostSteamId { get; set; }
    
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; private set; }
    
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
        bool isPrivate,
        string? passwordHash
    )
    {
        RoomCode = roomCode;
        RoomName = roomName;
        SteamLobbyId = steamLobbyId;
        HostSteamId = hostSteamId;
        CurrentPlayers = 1;
        MaxPlayers = 2;
        IsPrivate = isPrivate;
        PasswordHash = passwordHash;
        Status = "ACTIVE";
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
        Status = "DELETING";
        ExpiresAt = DateTimeOffset.UtcNow.Add(ttl);
    }

    public void Close()
    {
        Status = "CLOSED";
        ExpiresAt = DateTimeOffset.UtcNow;
    }
}