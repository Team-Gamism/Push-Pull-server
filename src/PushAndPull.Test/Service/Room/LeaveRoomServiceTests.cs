using Moq;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;
using EntityRoom = PushAndPull.Domain.Room.Entity.Room;

namespace Tests.Service.Room;

public class LeaveRoomServiceTests
{
    private const ulong HostSteamId = 76561198000000001UL;
    private const ulong GuestSteamId = 76561198000000002UL;

    public class WhenTheRoomDoesNotExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly LeaveRoomService _sut;

        private const string RoomCode = "NOTEXIST";

        public WhenTheRoomDoesNotExist()
        {
            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityRoom?)null);

            _sut = new LeaveRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new LeaveRoomCommand(RoomCode, GuestSteamId)));
        }
    }

    public class WhenTheRoomIsNotActive
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly LeaveRoomService _sut;

        private const string RoomCode = "CLOSED1";

        public WhenTheRoomIsNotActive()
        {
            var closedRoom = new EntityRoom(RoomCode, "Closed Room", 111UL, HostSteamId, false, null);
            closedRoom.Close();

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(closedRoom);

            _sut = new LeaveRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotActiveException()
        {
            await Assert.ThrowsAsync<RoomNotActiveException>(
                () => _sut.ExecuteAsync(new LeaveRoomCommand(RoomCode, GuestSteamId)));
        }
    }

    public class WhenTheHostLeavesTheRoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly LeaveRoomService _sut;

        private const string RoomCode = "HOST01";

        public WhenTheHostLeavesTheRoom()
        {
            var room = new EntityRoom(RoomCode, "Host Room", 111UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _sut = new LeaveRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ClosesTheRoom()
        {
            await _sut.ExecuteAsync(new LeaveRoomCommand(RoomCode, HostSteamId));

            _roomRepositoryMock.Verify(r => r.CloseAsync(RoomCode, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task It_DoesNotRemoveAGuest()
        {
            await _sut.ExecuteAsync(new LeaveRoomCommand(RoomCode, HostSteamId));

            _roomRepositoryMock.Verify(
                r => r.TryRemoveGuestAsync(It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    public class WhenTheGuestLeavesTheRoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly LeaveRoomService _sut;

        private const string RoomCode = "GUEST1";

        public WhenTheGuestLeavesTheRoom()
        {
            var room = new EntityRoom(RoomCode, "Guest Room", 111UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _roomRepositoryMock
                .Setup(r => r.TryRemoveGuestAsync(RoomCode, GuestSteamId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _sut = new LeaveRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_RemovesTheGuest()
        {
            await _sut.ExecuteAsync(new LeaveRoomCommand(RoomCode, GuestSteamId));

            _roomRepositoryMock.Verify(r => r.TryRemoveGuestAsync(RoomCode, GuestSteamId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task It_DoesNotCloseTheRoom()
        {
            await _sut.ExecuteAsync(new LeaveRoomCommand(RoomCode, GuestSteamId));

            _roomRepositoryMock.Verify(
                r => r.CloseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    public class WhenANonParticipantLeavesTheRoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly LeaveRoomService _sut;

        private const string RoomCode = "STRNGR";
        private const ulong StrangerSteamId = 76561198000000003UL;

        public WhenANonParticipantLeavesTheRoom()
        {
            var room = new EntityRoom(RoomCode, "Some Room", 111UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _roomRepositoryMock
                .Setup(r => r.TryRemoveGuestAsync(RoomCode, StrangerSteamId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _sut = new LeaveRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotParticipantException()
        {
            await Assert.ThrowsAsync<RoomNotParticipantException>(
                () => _sut.ExecuteAsync(new LeaveRoomCommand(RoomCode, StrangerSteamId)));
        }
    }

    public class WhenRemovalFailsBecauseRoomDisappearedConcurrently
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly LeaveRoomService _sut;

        private const string RoomCode = "GONE01";

        public WhenRemovalFailsBecauseRoomDisappearedConcurrently()
        {
            var activeRoom = new EntityRoom(RoomCode, "Disappearing Room", 111UL, HostSteamId, false, null);

            _roomRepositoryMock
                .SetupSequence(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeRoom)
                .ReturnsAsync((EntityRoom?)null);

            _roomRepositoryMock
                .Setup(r => r.TryRemoveGuestAsync(RoomCode, GuestSteamId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _sut = new LeaveRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new LeaveRoomCommand(RoomCode, GuestSteamId)));
        }
    }
}
