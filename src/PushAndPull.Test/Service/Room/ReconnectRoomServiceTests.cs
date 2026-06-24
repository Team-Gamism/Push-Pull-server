using Moq;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;
using EntityRoom = PushAndPull.Domain.Room.Entity.Room;

namespace Tests.Service.Room;

public class ReconnectRoomServiceTests
{
    private const ulong HostSteamId = 76561198000000001UL;
    private const ulong GuestSteamId = 76561198000000003UL;
    private const ulong OtherSteamId = 76561198000000002UL;

    private static readonly DateTimeOffset Now = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    // GuestSteamId는 리포지토리의 ExecuteUpdate로만 채워지므로, 단위 테스트에서는 private setter를 reflection으로 설정한다.
    private static EntityRoom CreateRoomWithGuest(string roomCode, ulong steamLobbyId, ulong hostSteamId, ulong guestSteamId)
    {
        var room = new EntityRoom(roomCode, "Room", steamLobbyId, hostSteamId, false, null);
        typeof(EntityRoom)
            .GetProperty(nameof(EntityRoom.GuestSteamId))!
            .SetValue(room, guestSteamId);
        return room;
    }

    public class WhenTheHostReconnects
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly ReconnectRoomService _sut;

        private const string RoomCode = "RECN01";
        private const ulong SteamLobbyId = 444UL;

        public WhenTheHostReconnects()
        {
            var room = new EntityRoom(RoomCode, "Active Room", SteamLobbyId, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _roomRepositoryMock
                .Setup(r => r.UpdateHeartbeatAsync(RoomCode, HostSteamId, Now, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _sut = new ReconnectRoomService(_roomRepositoryMock.Object, new FixedTimeProvider(Now));
        }

        [Fact]
        public async Task It_ReturnsTheSteamLobbyIdWithHostRole()
        {
            var result = await _sut.ExecuteAsync(new ReconnectRoomCommand(RoomCode, HostSteamId));

            Assert.Equal(SteamLobbyId, result.SteamLobbyId);
            Assert.Equal("Host", result.Role);
        }

        [Fact]
        public async Task It_ResetsTheHeartbeat()
        {
            await _sut.ExecuteAsync(new ReconnectRoomCommand(RoomCode, HostSteamId));

            _roomRepositoryMock.Verify(
                r => r.UpdateHeartbeatAsync(RoomCode, HostSteamId, Now, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    public class WhenTheGuestReconnects
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly ReconnectRoomService _sut;

        private const string RoomCode = "RECN02";
        private const ulong SteamLobbyId = 555UL;

        public WhenTheGuestReconnects()
        {
            var room = CreateRoomWithGuest(RoomCode, SteamLobbyId, HostSteamId, GuestSteamId);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _roomRepositoryMock
                .Setup(r => r.UpdateHeartbeatAsync(RoomCode, GuestSteamId, Now, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _sut = new ReconnectRoomService(_roomRepositoryMock.Object, new FixedTimeProvider(Now));
        }

        [Fact]
        public async Task It_ReturnsTheSteamLobbyIdWithGuestRole()
        {
            var result = await _sut.ExecuteAsync(new ReconnectRoomCommand(RoomCode, GuestSteamId));

            Assert.Equal(SteamLobbyId, result.SteamLobbyId);
            Assert.Equal("Guest", result.Role);
        }
    }

    public class WhenTheRoomDoesNotExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly ReconnectRoomService _sut;

        private const string RoomCode = "NOTEXIST";

        public WhenTheRoomDoesNotExist()
        {
            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityRoom?)null);

            _sut = new ReconnectRoomService(_roomRepositoryMock.Object, new FixedTimeProvider(Now));
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new ReconnectRoomCommand(RoomCode, HostSteamId)));
        }
    }

    public class WhenTheRoomIsNotActive
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly ReconnectRoomService _sut;

        private const string RoomCode = "CLOSED1";

        public WhenTheRoomIsNotActive()
        {
            var closedRoom = new EntityRoom(RoomCode, "Closed Room", 111UL, HostSteamId, false, null);
            closedRoom.Close();

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(closedRoom);

            _sut = new ReconnectRoomService(_roomRepositoryMock.Object, new FixedTimeProvider(Now));
        }

        [Fact]
        public async Task It_ThrowsRoomNotActiveException()
        {
            await Assert.ThrowsAsync<RoomNotActiveException>(
                () => _sut.ExecuteAsync(new ReconnectRoomCommand(RoomCode, HostSteamId)));
        }
    }

    public class WhenANonParticipantReconnects
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly ReconnectRoomService _sut;

        private const string RoomCode = "RECN03";

        public WhenANonParticipantReconnects()
        {
            var room = new EntityRoom(RoomCode, "Active Room", 666UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _sut = new ReconnectRoomService(_roomRepositoryMock.Object, new FixedTimeProvider(Now));
        }

        [Fact]
        public async Task It_ThrowsRoomNotParticipantException()
        {
            await Assert.ThrowsAsync<RoomNotParticipantException>(
                () => _sut.ExecuteAsync(new ReconnectRoomCommand(RoomCode, OtherSteamId)));
        }

        [Fact]
        public async Task It_DoesNotUpdateTheHeartbeat()
        {
            await Assert.ThrowsAsync<RoomNotParticipantException>(
                () => _sut.ExecuteAsync(new ReconnectRoomCommand(RoomCode, OtherSteamId)));

            _roomRepositoryMock.Verify(
                r => r.UpdateHeartbeatAsync(It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    public class WhenTheGuestSlotIsLostInARace
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly ReconnectRoomService _sut;

        private const string RoomCode = "RACE01";

        public WhenTheGuestSlotIsLostInARace()
        {
            var roomWithGuest = CreateRoomWithGuest(RoomCode, 777UL, HostSteamId, GuestSteamId);
            var roomWithoutGuest = new EntityRoom(RoomCode, "Room", 777UL, HostSteamId, false, null);

            _roomRepositoryMock
                .SetupSequence(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roomWithGuest)
                .ReturnsAsync(roomWithoutGuest);

            _roomRepositoryMock
                .Setup(r => r.UpdateHeartbeatAsync(RoomCode, GuestSteamId, Now, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _sut = new ReconnectRoomService(_roomRepositoryMock.Object, new FixedTimeProvider(Now));
        }

        [Fact]
        public async Task It_ThrowsRoomNotParticipantException()
        {
            await Assert.ThrowsAsync<RoomNotParticipantException>(
                () => _sut.ExecuteAsync(new ReconnectRoomCommand(RoomCode, GuestSteamId)));
        }
    }
}
