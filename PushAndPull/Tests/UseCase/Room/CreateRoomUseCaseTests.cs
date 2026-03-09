using Moq;
using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.Port.Output.Persistence;
using Server.Application.UseCase.Room;
using EntityRoom = Server.Domain.Entity.Room;

namespace Tests.UseCase.Room;

public class CreateRoomUseCaseTests
{
    public class WhenCreatingAPublicRoomWithoutAPassword
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IRoomCodeGenerator> _roomCodeGeneratorMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly CreateRoomUseCase _sut;

        private const string GeneratedCode = "ABC123";
        private readonly CreateRoomCommand _command = new(
            LobbyId: 111222333UL,
            RoomName: "Test Room",
            IsPrivate: false,
            Password: null,
            HostSteamId: 76561198000000001UL
        );

        public WhenCreatingAPublicRoomWithoutAPassword()
        {
            _roomCodeGeneratorMock.Setup(g => g.Generate()).Returns(GeneratedCode);

            _sut = new CreateRoomUseCase(
                _roomRepositoryMock.Object,
                _roomCodeGeneratorMock.Object,
                _passwordHasherMock.Object
            );
        }

        [Fact]
        public async Task It_ReturnsTheGeneratedRoomCode()
        {
            var result = await _sut.ExecuteAsync(_command);

            Assert.Equal(GeneratedCode, result.RoomCode);
        }

        [Fact]
        public async Task It_SavesTheRoomToRepository()
        {
            await _sut.ExecuteAsync(_command);

            _roomRepositoryMock.Verify(r => r.CreateAsync(
                It.Is<EntityRoom>(room =>
                    room.RoomCode == GeneratedCode &&
                    room.RoomName == _command.RoomName &&
                    room.IsPrivate == false &&
                    room.PasswordHash == null
                )), Times.Once);
        }

        [Fact]
        public async Task It_DoesNotHashAnyPassword()
        {
            await _sut.ExecuteAsync(_command);

            _passwordHasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        }
    }

    public class WhenCreatingAPrivateRoomWithAPassword
    {
        private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
        private readonly Mock<IRoomCodeGenerator> _roomCodeGeneratorMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly CreateRoomUseCase _sut;

        private const string GeneratedCode = "XYZ789";
        private const string RawPassword = "secret1234";
        private const string HashedPassword = "hashed-secret1234";

        private readonly CreateRoomCommand _command = new(
            LobbyId: 999888777UL,
            RoomName: "Private Room",
            IsPrivate: true,
            Password: RawPassword,
            HostSteamId: 76561198000000002UL
        );

        public WhenCreatingAPrivateRoomWithAPassword()
        {
            _roomCodeGeneratorMock.Setup(g => g.Generate()).Returns(GeneratedCode);
            _passwordHasherMock.Setup(h => h.Hash(RawPassword)).Returns(HashedPassword);

            _sut = new CreateRoomUseCase(
                _roomRepositoryMock.Object,
                _roomCodeGeneratorMock.Object,
                _passwordHasherMock.Object
            );
        }

        [Fact]
        public async Task It_SavesTheHashedPassword()
        {
            await _sut.ExecuteAsync(_command);

            _roomRepositoryMock.Verify(r => r.CreateAsync(
                It.Is<EntityRoom>(room =>
                    room.PasswordHash == HashedPassword
                )), Times.Once);
        }

        [Fact]
        public async Task It_ReturnsTheGeneratedRoomCode()
        {
            var result = await _sut.ExecuteAsync(_command);

            Assert.Equal(GeneratedCode, result.RoomCode);
        }
    }
}
