using Moq;
using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.Port.Output.Persistence;
using Server.Application.UseCase.Room;
using Server.Domain.Exception.Room;
using EntityRoom = Server.Domain.Entity.Room;

namespace Tests.UseCase.Room;

public class JoinRoomUseCaseTests
{
    public class WhenTheRoomDoesNotExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomUseCase _sut;

        private const string RoomCode = "NOTEXIST";

        public WhenTheRoomDoesNotExist()
        {
            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode))
                .ReturnsAsync((EntityRoom?)null);

            _sut = new JoinRoomUseCase(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null)));
        }
    }

    public class WhenTheRoomIsNotActive
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomUseCase _sut;

        private const string RoomCode = "CLOSED1";

        public WhenTheRoomIsNotActive()
        {
            var closedRoom = new EntityRoom("CLOSED1", "Closed Room", 111UL, 76561198000000001UL, false, null);
            closedRoom.Close();

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode))
                .ReturnsAsync(closedRoom);

            _sut = new JoinRoomUseCase(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotActiveException()
        {
            await Assert.ThrowsAsync<RoomNotActiveException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null)));
        }
    }

    public class WhenTheWrongPasswordIsProvidedForAPrivateRoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomUseCase _sut;

        private const string RoomCode = "PRIV01";
        private const string WrongPassword = "wrong-password";
        private const string StoredHash = "correct-hash";

        public WhenTheWrongPasswordIsProvidedForAPrivateRoom()
        {
            var privateRoom = new EntityRoom(RoomCode, "Private Room", 222UL, 76561198000000001UL, true, StoredHash);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode))
                .ReturnsAsync(privateRoom);

            _passwordHasherMock
                .Setup(h => h.Verify(WrongPassword, StoredHash))
                .Returns(false);

            _sut = new JoinRoomUseCase(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsInvalidOperationExceptionWithInvalidPasswordMessage()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, WrongPassword)));

            Assert.Equal("INVALID_PASSWORD", ex.Message);
        }
    }

    public class WhenIncrementFailsBecauseRoomDisappearedConcurrently
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomUseCase _sut;

        private const string RoomCode = "GONE01";

        public WhenIncrementFailsBecauseRoomDisappearedConcurrently()
        {
            var activeRoom = new EntityRoom(RoomCode, "Disappearing Room", 444UL, 76561198000000001UL, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode))
                .ReturnsAsync(activeRoom);

            _roomRepositoryMock
                .Setup(r => r.IncrementPlayerCountAsync(RoomCode))
                .ReturnsAsync(false);

            // 재조회 시 방이 사라진 상태
            _roomRepositoryMock
                .SetupSequence(r => r.GetAsync(RoomCode))
                .ReturnsAsync(activeRoom)
                .ReturnsAsync((EntityRoom?)null);

            _sut = new JoinRoomUseCase(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null)));
        }
    }

    public class WhenIncrementFailsBecauseRoomBecameInactiveConcurrently
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomUseCase _sut;

        private const string RoomCode = "CLOS02";

        public WhenIncrementFailsBecauseRoomBecameInactiveConcurrently()
        {
            var activeRoom = new EntityRoom(RoomCode, "Closing Room", 555UL, 76561198000000001UL, false, null);
            var closedRoom = new EntityRoom(RoomCode, "Closing Room", 555UL, 76561198000000001UL, false, null);
            closedRoom.Close();

            _roomRepositoryMock
                .SetupSequence(r => r.GetAsync(RoomCode))
                .ReturnsAsync(activeRoom)
                .ReturnsAsync(closedRoom);

            _roomRepositoryMock
                .Setup(r => r.IncrementPlayerCountAsync(RoomCode))
                .ReturnsAsync(false);

            _sut = new JoinRoomUseCase(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotActiveException()
        {
            await Assert.ThrowsAsync<RoomNotActiveException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null)));
        }
    }

    public class WhenIncrementFailsBecauseRoomIsFullConcurrently
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomUseCase _sut;

        private const string RoomCode = "FULL01";

        public WhenIncrementFailsBecauseRoomIsFullConcurrently()
        {
            var activeRoom = new EntityRoom(RoomCode, "Full Room", 666UL, 76561198000000001UL, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode))
                .ReturnsAsync(activeRoom);

            _roomRepositoryMock
                .Setup(r => r.IncrementPlayerCountAsync(RoomCode))
                .ReturnsAsync(false);

            _sut = new JoinRoomUseCase(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsFullRoomException()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null)));

            Assert.Equal("FULL_ROOM", ex.Message);
        }
    }

    public class WhenAllConditionsAreValidForJoiningARoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomUseCase _sut;

        private const string RoomCode = "OPEN01";
        private readonly EntityRoom _activeRoom;

        public WhenAllConditionsAreValidForJoiningARoom()
        {
            _activeRoom = new EntityRoom(RoomCode, "Open Room", 333UL, 76561198000000001UL, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode))
                .ReturnsAsync(_activeRoom);

            _roomRepositoryMock
                .Setup(r => r.IncrementPlayerCountAsync(RoomCode))
                .ReturnsAsync(true);

            _sut = new JoinRoomUseCase(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_CallsIncrementPlayerCount()
        {
            await _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null));

            _roomRepositoryMock.Verify(r => r.IncrementPlayerCountAsync(RoomCode), Times.Once);
        }

        [Fact]
        public async Task It_IncreasesCurrentPlayerCount()
        {
            var before = _activeRoom.CurrentPlayers;

            await _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null));

            Assert.Equal(before + 1, _activeRoom.CurrentPlayers);
        }
    }
}
