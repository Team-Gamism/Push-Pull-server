using Moq;
using PushAndPull.Domain.Room.Repository;
using PushAndPull.Domain.Room.Service;
using EntityRoom = PushAndPull.Domain.Room.Entity.Room;

namespace Tests.Service.Room;

public class GetAllRoomUseCaseTests
{
    public class WhenMultipleActiveRoomsExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly GetAllRoomUseCase _sut;

        private readonly IReadOnlyList<EntityRoom> _rooms;

        public WhenMultipleActiveRoomsExist()
        {
            _rooms = new List<EntityRoom>
            {
                new EntityRoom("AAA001", "Room A", 111UL, 76561198000000001UL, false, null),
                new EntityRoom("BBB002", "Room B", 222UL, 76561198000000002UL, true, "hash"),
            };

            _roomRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_rooms);

            _sut = new GetAllRoomUseCase(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ReturnsAllRooms()
        {
            var result = await _sut.ExecuteAsync();

            Assert.Equal(_rooms.Count, result.Rooms.Count);
        }

        [Fact]
        public async Task It_ReturnsCorrectRoomSummaries()
        {
            var result = await _sut.ExecuteAsync();

            Assert.Equal("Room A", result.Rooms[0].RoomName);
            Assert.Equal("AAA001", result.Rooms[0].RoomCode);
            Assert.Equal("Room B", result.Rooms[1].RoomName);
            Assert.Equal("BBB002", result.Rooms[1].RoomCode);
        }
    }

    public class WhenNoRoomsExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly GetAllRoomUseCase _sut;

        public WhenNoRoomsExist()
        {
            _roomRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityRoom>());

            _sut = new GetAllRoomUseCase(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ReturnsAnEmptyList()
        {
            var result = await _sut.ExecuteAsync();

            Assert.Empty(result.Rooms);
        }
    }
}
