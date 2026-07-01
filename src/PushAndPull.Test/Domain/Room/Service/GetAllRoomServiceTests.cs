using Moq;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;
using EntityRoom = PushAndPull.Domain.Room.Entity.Room;

namespace PushAndPull.Test.Domain.Room.Service;

public class GetAllRoomServiceTests
{
    public class WhenMultipleActiveRoomsExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly GetAllRoomService _sut;

        private readonly IReadOnlyList<EntityRoom> _rooms;

        public WhenMultipleActiveRoomsExist()
        {
            _rooms = new List<EntityRoom>
            {
                new EntityRoom("AAA001", "Room A", 111UL, 76561198000000001UL, false, null),
                new EntityRoom("BBB002", "Room B", 222UL, 76561198000000002UL, true, "hash"),
            };

            _roomRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_rooms);

            _sut = new GetAllRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ReturnsAllRooms()
        {
            var result = await _sut.ExecuteAsync(new GetAllRoomQuery());

            Assert.Equal(_rooms.Count, result.Rooms.Count);
        }

        [Fact]
        public async Task It_ReturnsCorrectRoomSummaries()
        {
            var result = await _sut.ExecuteAsync(new GetAllRoomQuery());

            Assert.Equal("Room A", result.Rooms[0].RoomName);
            Assert.Equal("AAA001", result.Rooms[0].RoomCode);
            Assert.Equal("Room B", result.Rooms[1].RoomName);
            Assert.Equal("BBB002", result.Rooms[1].RoomCode);
            Assert.Equal(_rooms[0].MaxPlayers, result.Rooms[0].MaxPlayers);
            Assert.False(result.Rooms[0].HasPassword);
            Assert.True(result.Rooms[1].HasPassword);
        }

        [Fact]
        public async Task It_HasNoNextPage()
        {
            var result = await _sut.ExecuteAsync(new GetAllRoomQuery(Page: 1, Size: 20));

            Assert.False(result.HasNext);
        }
    }

    public class WhenMoreRoomsExistThanThePageSize
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly GetAllRoomService _sut;

        private const int Size = 2;

        public WhenMoreRoomsExistThanThePageSize()
        {
            // size + 1개를 반환해 다음 페이지가 있음을 시뮬레이션한다.
            var rooms = new List<EntityRoom>
            {
                new EntityRoom("AAA001", "Room A", 111UL, 76561198000000001UL, false, null),
                new EntityRoom("BBB002", "Room B", 222UL, 76561198000000002UL, false, null),
                new EntityRoom("CCC003", "Room C", 333UL, 76561198000000003UL, false, null),
            };

            _roomRepositoryMock
                .Setup(r => r.GetAllAsync(0, Size + 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rooms);

            _sut = new GetAllRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_TruncatesToThePageSizeAndReportsANextPage()
        {
            var result = await _sut.ExecuteAsync(new GetAllRoomQuery(Page: 1, Size: Size));

            Assert.Equal(Size, result.Rooms.Count);
            Assert.True(result.HasNext);
        }
    }

    public class WhenAnOversizedPageIsRequested
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly GetAllRoomService _sut;

        public WhenAnOversizedPageIsRequested()
        {
            _roomRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityRoom>());

            _sut = new GetAllRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ClampsTheSizeToTheMaximum()
        {
            var result = await _sut.ExecuteAsync(new GetAllRoomQuery(Page: 1, Size: 500));

            Assert.Equal(50, result.Size);
            _roomRepositoryMock.Verify(r => r.GetAllAsync(0, 51, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task It_NormalizesNonPositivePagesToTheFirstPage()
        {
            var result = await _sut.ExecuteAsync(new GetAllRoomQuery(Page: 0, Size: 10));

            Assert.Equal(1, result.Page);
            _roomRepositoryMock.Verify(r => r.GetAllAsync(0, 11, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class WhenNoRoomsExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly GetAllRoomService _sut;

        public WhenNoRoomsExist()
        {
            _roomRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityRoom>());

            _sut = new GetAllRoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ReturnsAnEmptyList()
        {
            var result = await _sut.ExecuteAsync(new GetAllRoomQuery());

            Assert.Empty(result.Rooms);
            Assert.False(result.HasNext);
        }
    }
}
