using Moq;
using PushAndPull.Domain.Room.Exception;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;
using PushAndPull.Global.Service;
using EntityRoom = PushAndPull.Domain.Room.Entity.Room;

namespace PushAndPull.Test.Domain.Room.Service;

public class JoinRoomServiceTests
{
    private const ulong HostSteamId = 76561198000000001UL;
    private const ulong JoinerSteamId = 76561198000000002UL;

    public class WhenTheRoomDoesNotExist
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "NOTEXIST";

        public WhenTheRoomDoesNotExist()
        {
            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityRoom?)null);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId)));
        }
    }

    public class WhenTheRoomIsNotActive
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "CLOSED1";

        public WhenTheRoomIsNotActive()
        {
            var closedRoom = new EntityRoom(RoomCode, "Closed Room", 111UL, HostSteamId, false, null);
            closedRoom.Close();

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(closedRoom);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotActiveException()
        {
            await Assert.ThrowsAsync<RoomNotActiveException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId)));
        }
    }

    public class WhenTheHostJoinsTheirOwnRoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "HOST01";

        public WhenTheHostJoinsTheirOwnRoom()
        {
            var room = new EntityRoom(RoomCode, "Host Room", 111UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsAlreadyJoinedRoomException()
        {
            await Assert.ThrowsAsync<AlreadyJoinedRoomException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, HostSteamId)));
        }

        [Fact]
        public async Task It_DoesNotAttemptToJoin()
        {
            await Assert.ThrowsAsync<AlreadyJoinedRoomException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, HostSteamId)));

            _roomRepositoryMock.Verify(
                r => r.TryJoinAsync(It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    public class WhenAPrivateRoomIsJoinedWithoutAPassword
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "PRIV02";

        public WhenAPrivateRoomIsJoinedWithoutAPassword()
        {
            var privateRoom = new EntityRoom(RoomCode, "Private Room", 222UL, HostSteamId, true, "some-hash");

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(privateRoom);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsPasswordRequiredException()
        {
            await Assert.ThrowsAsync<PasswordRequiredException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId)));
        }
    }

    public class WhenAPrivateRoomWithoutAPasswordIsJoined
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "PRIV04";

        public WhenAPrivateRoomWithoutAPasswordIsJoined()
        {
            var privateRoomWithoutPassword = new EntityRoom(RoomCode, "Hidden Room", 888UL, HostSteamId, true, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(privateRoomWithoutPassword);

            _roomRepositoryMock
                .Setup(r => r.TryJoinAsync(RoomCode, JoinerSteamId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_JoinsWithoutPasswordVerification()
        {
            await _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId));

            _passwordHasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _roomRepositoryMock.Verify(r => r.TryJoinAsync(RoomCode, JoinerSteamId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class WhenAPublicRoomWithAPasswordIsJoinedWithoutAPassword
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "PUBL01";

        public WhenAPublicRoomWithAPasswordIsJoinedWithoutAPassword()
        {
            var publicRoomWithPassword = new EntityRoom(RoomCode, "Locked Public Room", 999UL, HostSteamId, false, "some-hash");

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(publicRoomWithPassword);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsPasswordRequiredException()
        {
            await Assert.ThrowsAsync<PasswordRequiredException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId)));
        }
    }

    public class WhenTheWrongPasswordIsProvidedForAPrivateRoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "PRIV01";
        private const string WrongPassword = "wrong-password";
        private const string StoredHash = "correct-hash";

        public WhenTheWrongPasswordIsProvidedForAPrivateRoom()
        {
            var privateRoom = new EntityRoom(RoomCode, "Private Room", 222UL, HostSteamId, true, StoredHash);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(privateRoom);

            _passwordHasherMock
                .Setup(h => h.Verify(WrongPassword, StoredHash))
                .Returns(false);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsInvalidPasswordException()
        {
            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, WrongPassword, JoinerSteamId)));
        }
    }

    public class WhenJoinFailsBecauseRoomDisappearedConcurrently
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "GONE01";

        public WhenJoinFailsBecauseRoomDisappearedConcurrently()
        {
            var activeRoom = new EntityRoom(RoomCode, "Disappearing Room", 444UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.TryJoinAsync(RoomCode, JoinerSteamId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _roomRepositoryMock
                .SetupSequence(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeRoom)
                .ReturnsAsync((EntityRoom?)null);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotFoundException()
        {
            await Assert.ThrowsAsync<RoomNotFoundException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId)));
        }
    }

    public class WhenJoinFailsBecauseRoomBecameInactiveConcurrently
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "CLOS02";

        public WhenJoinFailsBecauseRoomBecameInactiveConcurrently()
        {
            var activeRoom = new EntityRoom(RoomCode, "Closing Room", 555UL, HostSteamId, false, null);
            var closedRoom = new EntityRoom(RoomCode, "Closing Room", 555UL, HostSteamId, false, null);
            closedRoom.Close();

            _roomRepositoryMock
                .SetupSequence(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeRoom)
                .ReturnsAsync(closedRoom);

            _roomRepositoryMock
                .Setup(r => r.TryJoinAsync(RoomCode, JoinerSteamId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomNotActiveException()
        {
            await Assert.ThrowsAsync<RoomNotActiveException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId)));
        }
    }

    public class WhenJoinFailsBecauseRoomIsFullConcurrently
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "FULL01";

        public WhenJoinFailsBecauseRoomIsFullConcurrently()
        {
            var activeRoom = new EntityRoom(RoomCode, "Full Room", 666UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeRoom);

            _roomRepositoryMock
                .Setup(r => r.TryJoinAsync(RoomCode, JoinerSteamId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_ThrowsRoomFullException()
        {
            await Assert.ThrowsAsync<RoomFullException>(
                () => _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId)));
        }
    }

    public class WhenAllConditionsAreValidForJoiningARoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "OPEN01";
        private readonly EntityRoom _activeRoom;

        public WhenAllConditionsAreValidForJoiningARoom()
        {
            _activeRoom = new EntityRoom(RoomCode, "Open Room", 333UL, HostSteamId, false, null);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_activeRoom);

            _roomRepositoryMock
                .Setup(r => r.TryJoinAsync(RoomCode, JoinerSteamId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_CallsTryJoin()
        {
            await _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId));

            _roomRepositoryMock.Verify(r => r.TryJoinAsync(RoomCode, JoinerSteamId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task It_DoesNotThrowAnyException()
        {
            await _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId));
        }

        [Fact]
        public async Task It_ReturnsTheSteamLobbyId()
        {
            var result = await _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, null, JoinerSteamId));

            Assert.Equal(_activeRoom.SteamLobbyId, result.SteamLobbyId);
        }
    }

    public class WhenCorrectPasswordIsProvidedForAPrivateRoom
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly JoinRoomService _sut;

        private const string RoomCode = "PRIV03";
        private const string CorrectPassword = "correct-password";
        private const string StoredHash = "correct-hash";

        public WhenCorrectPasswordIsProvidedForAPrivateRoom()
        {
            var privateRoom = new EntityRoom(RoomCode, "Private Room", 777UL, HostSteamId, true, StoredHash);

            _roomRepositoryMock
                .Setup(r => r.GetAsync(RoomCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(privateRoom);

            _passwordHasherMock
                .Setup(h => h.Verify(CorrectPassword, StoredHash))
                .Returns(true);

            _roomRepositoryMock
                .Setup(r => r.TryJoinAsync(RoomCode, JoinerSteamId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _sut = new JoinRoomService(_roomRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task It_CallsTryJoin()
        {
            await _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, CorrectPassword, JoinerSteamId));

            _roomRepositoryMock.Verify(r => r.TryJoinAsync(RoomCode, JoinerSteamId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task It_DoesNotThrowAnyException()
        {
            await _sut.ExecuteAsync(new JoinRoomCommand(RoomCode, CorrectPassword, JoinerSteamId));
        }
    }
}
