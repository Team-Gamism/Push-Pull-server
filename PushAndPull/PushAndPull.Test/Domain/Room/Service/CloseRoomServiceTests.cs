using Moq;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;
using EntityRoom = PushAndPull.Domain.Room.Entity.Room;

namespace PushAndPull.Test.Domain.Room.Service;

public class CloseRoomServiceTests
{
    private const ulong HostSteamId = 76561198000000001UL;
    private const ulong OtherSteamId = 76561198000000002UL;

    public class WhenTheRoomDoesNotExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly CloseRoomService _sut;

        private const string RoomCode = "NOTEXIST";

        public WhenTheRoomDoesNotExist()
        {
            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityRoom?)null);

            _sut = new CloseRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new CloseRoomCommand(RoomCode, HostSteamId)));
        }
    }

    public class WhenANonHostTriesToCloseTheRoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly CloseRoomService _sut;

        private const string RoomCode = "ROOM01";

        public WhenANonHostTriesToCloseTheRoom()
        {
            var room = new EntityRoom(RoomCode, "Some Room", 111UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _sut = new CloseRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ThrowsNotRoomHostException()
        {
            await Assert.ThrowsAsync<NotRoomHostException>(
                () => _sut.ExecuteAsync(new CloseRoomCommand(RoomCode, OtherSteamId)));
        }

        [Fact]
        public async Task It_DoesNotCloseTheRoom()
        {
            await Assert.ThrowsAsync<NotRoomHostException>(
                () => _sut.ExecuteAsync(new CloseRoomCommand(RoomCode, OtherSteamId)));

            _roomRepositoryMock.Verify(
                r => r.CloseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    public class WhenTheHostClosesTheRoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly CloseRoomService _sut;

        private const string RoomCode = "ROOM02";

        public WhenTheHostClosesTheRoom()
        {
            var room = new EntityRoom(RoomCode, "Host Room", 111UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _sut = new CloseRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ClosesTheRoom()
        {
            await _sut.ExecuteAsync(new CloseRoomCommand(RoomCode, HostSteamId));

            _roomRepositoryMock.Verify(r => r.CloseAsync(RoomCode, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
