namespace PushAndPull.Domain.Room.Config;

public class RoomCleanupOptions
{
    public int SweepIntervalSeconds { get; init; } = 60;
    public int HeartbeatTimeoutSeconds { get; init; } = 120;
}
