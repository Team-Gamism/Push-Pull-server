using Moq;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;
using EntityRoom = PushAndPull.Domain.Room.Entity.Room;

namespace Tests.Service.Room;

public class GetRoomUseCaseTests
{
    public class WhenAnEmptyRoomCodeIsProvided
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly GetRoomUseCase _sut;

        public WhenAnEmptyRoomCodeIsProvided()
        {
            _sut = new GetRoomUseCase(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.ExecuteAsync(new GetRoomCommand("")));
        }
    }

    public class WhenTheRoomDoesNotExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly GetRoomUseCase _sut;

        private const string RoomCode = "NOTFOUND";

        public WhenTheRoomDoesNotExist()
        {
            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode))
                .ReturnsAsync((EntityRoom?)null);

            _sut = new GetRoomUseCase(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new GetRoomCommand(RoomCode)));
        }
    }

    public class WhenTheRoomExists
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly GetRoomUseCase _sut;

        private const string RoomCode = "EXIST1";
        private const string RoomName = "Existing Room";
        private readonly EntityRoom _room;

        public WhenTheRoomExists()
        {
            _room = new EntityRoom(RoomCode, RoomName, 444UL, 76561198000000001UL, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode))
                .ReturnsAsync(_room);

            _sut = new GetRoomUseCase(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ReturnsRoomInfo()
        {
            var result = await _sut.ExecuteAsync(new GetRoomCommand(RoomCode));

            Assert.Equal(RoomCode, result.RoomCode);
            Assert.Equal(RoomName, result.RoomName);
            Assert.Equal(_room.CurrentPlayers, result.CurrentPlayers);
            Assert.Equal(_room.IsPrivate, result.IsPrivate);
        }
    }
}
