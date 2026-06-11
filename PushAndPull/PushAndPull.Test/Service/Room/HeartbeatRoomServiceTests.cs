using Moq;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;
using EntityRoom = PushAndPull.Domain.Room.Entity.Room;

namespace Tests.Service.Room;

public class HeartbeatRoomServiceTests
{
    private const ulong HostSteamId = 76561198000000001UL;
    private const ulong OtherSteamId = 76561198000000002UL;

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    public class WhenTheHostSendsAHeartbeat
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly HeartbeatRoomService _sut;

        private const string RoomCode = "BEAT01";
        private static readonly DateTimeOffset Now = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

        public WhenTheHostSendsAHeartbeat()
        {
            _roomRepositoryMock
                .Setup(r => r.UpdateHeartbeatAsync(RoomCode, HostSteamId, Now, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _sut = new HeartbeatRoomService(_roomRepositoryMock.Object, new FixedTimeProvider(Now));
        }

        [Fact]
        public async Task It_UpdatesTheHeartbeatWithTheCurrentTime()
        {
            await _sut.ExecuteAsync(new HeartbeatRoomCommand(RoomCode, HostSteamId));

            _roomRepositoryMock.Verify(
                r => r.UpdateHeartbeatAsync(RoomCode, HostSteamId, Now, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    public class WhenTheRoomDoesNotExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly HeartbeatRoomService _sut;

        private const string RoomCode = "NOTEXIST";

        public WhenTheRoomDoesNotExist()
        {
            _roomRepositoryMock
                .Setup(r => r.UpdateHeartbeatAsync(RoomCode, HostSteamId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityRoom?)null);

            _sut = new HeartbeatRoomService(_roomRepositoryMock.Object, TimeProvider.System);
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new HeartbeatRoomCommand(RoomCode, HostSteamId)));
        }
    }

    public class WhenANonHostSendsAHeartbeat
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly HeartbeatRoomService _sut;

        private const string RoomCode = "BEAT02";

        public WhenANonHostSendsAHeartbeat()
        {
            var room = new EntityRoom(RoomCode, "Host Room", 111UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.UpdateHeartbeatAsync(RoomCode, OtherSteamId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _sut = new HeartbeatRoomService(_roomRepositoryMock.Object, TimeProvider.System);
        }

        [Fact]
        public async Task It_ThrowsNotRoomHostException()
        {
            await Assert.ThrowsAsync<NotRoomHostException>(
                () => _sut.ExecuteAsync(new HeartbeatRoomCommand(RoomCode, OtherSteamId)));
        }
    }

    public class WhenTheRoomIsNotActive
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly HeartbeatRoomService _sut;

        private const string RoomCode = "BEAT03";

        public WhenTheRoomIsNotActive()
        {
            var closedRoom = new EntityRoom(RoomCode, "Closed Room", 111UL, HostSteamId, false, null);
            closedRoom.Close();

            _roomRepositoryMock
                .Setup(r => r.UpdateHeartbeatAsync(RoomCode, HostSteamId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(closedRoom);

            _sut = new HeartbeatRoomService(_roomRepositoryMock.Object, TimeProvider.System);
        }

        [Fact]
        public async Task It_ThrowsRoomNotActiveException()
        {
            await Assert.ThrowsAsync<RoomNotActiveException>(
                () => _sut.ExecuteAsync(new HeartbeatRoomCommand(RoomCode, HostSteamId)));
        }
    }
}
