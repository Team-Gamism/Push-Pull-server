using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PushAndPull.Domain.Room.Config;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service;

namespace PushAndPull.Test.Domain.Room.Service;

public class StaleRoomCleanupServiceTests
{
    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    public class WhenASweepRuns
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly StaleRoomCleanupService _sut;

        private static readonly DateTimeOffset Now = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);
        private const int HeartbeatTimeoutSeconds = 120;

        public WhenASweepRuns()
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => _roomRepositoryMock.Object);
            var provider = services.BuildServiceProvider();

            _sut = new StaleRoomCleanupService(
                provider.GetRequiredService<IServiceScopeFactory>(),
                Options.Create(new RoomCleanupOptions
                {
                    SweepIntervalSeconds = 60,
                    HeartbeatTimeoutSeconds = HeartbeatTimeoutSeconds
                }),
                new FixedTimeProvider(Now),
                NullLogger<StaleRoomCleanupService>.Instance
            );
        }

        [Fact]
        public async Task It_ClosesRoomsStaleSinceTheHeartbeatTimeout()
        {
            await _sut.SweepOnceAsync(CancellationToken.None);

            var expectedCutoff = Now.AddSeconds(-HeartbeatTimeoutSeconds);
            _roomRepositoryMock.Verify(
                r => r.CloseStaleRoomsAsync(expectedCutoff, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task It_FreesGuestSlotsStaleSinceTheHeartbeatTimeout()
        {
            await _sut.SweepOnceAsync(CancellationToken.None);

            var expectedCutoff = Now.AddSeconds(-HeartbeatTimeoutSeconds);
            _roomRepositoryMock.Verify(
                r => r.FreeStaleGuestsAsync(expectedCutoff, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
