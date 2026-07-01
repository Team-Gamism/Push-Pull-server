using Microsoft.Extensions.Options;
using PushAndPull.Domain.Room.Config;
using PushAndPull.Domain.Room.Repository.Interface;

namespace PushAndPull.Domain.Room.Service;

public class StaleRoomCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RoomCleanupOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<StaleRoomCleanupService> _logger;

    public StaleRoomCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<RoomCleanupOptions> options,
        TimeProvider timeProvider,
        ILogger<StaleRoomCleanupService> logger
        )
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(_options.SweepIntervalSeconds), _timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Stale room sweep failed");
            }
        }
    }

    public async Task SweepOnceAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();

        var cutoff = _timeProvider.GetUtcNow()
            .AddSeconds(-_options.HeartbeatTimeoutSeconds);

        var closed = await roomRepository.CloseStaleRoomsAsync(cutoff, ct);
        if (closed > 0)
            _logger.LogInformation("Closed {Count} stale rooms", closed);

        var freed = await roomRepository.FreeStaleGuestsAsync(cutoff, ct);
        if (freed > 0)
            _logger.LogInformation("Freed {Count} stale guest slots", freed);
    }
}
