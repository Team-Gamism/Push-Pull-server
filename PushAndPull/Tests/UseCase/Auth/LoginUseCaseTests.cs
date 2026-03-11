using Moq;
using Server.Application.Port.Output;
using Server.Application.Port.Output.Persistence;
using Server.Application.UseCase.Auth;
using Server.Domain.Entity;
using Server.Application.Port.Input;

namespace Tests.UseCase.Auth;

public class LoginUseCaseTests
{
    public class WhenANewUserLogsInForTheFirstTime
    {
        private readonly Mock<IAuthTicketValidator> _validatorMock = new();
        private readonly Mock<ISessionService> _sessionServiceMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly LoginUseCase _sut;

        private const string Ticket = "valid-ticket";
        private const string Nickname = "TestPlayer";
        private const ulong SteamId = 76561198000000001UL;

        public WhenANewUserLogsInForTheFirstTime()
        {
            _validatorMock
                .Setup(v => v.ValidateAsync(Ticket))
                .ReturnsAsync(new AuthTicketValidationResult(SteamId, SteamId, false, false));

            _userRepositoryMock
                .Setup(r => r.GetBySteamIdAsync(SteamId, CancellationToken.None))
                .ReturnsAsync((User?)null);

            var session = new PlayerSession(SteamId, TimeSpan.FromDays(15));
            _sessionServiceMock
                .Setup(s => s.CreateAsync(SteamId, TimeSpan.FromDays(15)))
                .ReturnsAsync(session);

            _sut = new LoginUseCase(_validatorMock.Object, _sessionServiceMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task It_CreatesANewUser()
        {
            await _sut.ExecuteAsync(new LoginCommand(Ticket, Nickname));

            _userRepositoryMock.Verify(r => r.CreateAsync(
                It.Is<User>(u => u.SteamId == SteamId && u.Nickname == Nickname),
                CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task It_ReturnsASessionId()
        {
            var result = await _sut.ExecuteAsync(new LoginCommand(Ticket, Nickname));

            Assert.NotEmpty(result.SessionId);
        }

        [Fact]
        public async Task It_DoesNotCallUpdateUser()
        {
            await _sut.ExecuteAsync(new LoginCommand(Ticket, Nickname));

            _userRepositoryMock.Verify(r => r.UpdateAsync(
                It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<DateTime>(), CancellationToken.None), Times.Never);
        }
    }

    public class WhenAnExistingUserLogsInAgain
    {
        private readonly Mock<IAuthTicketValidator> _validatorMock = new();
        private readonly Mock<ISessionService> _sessionServiceMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly LoginUseCase _sut;

        private const string Ticket = "valid-ticket";
        private const string NewNickname = "UpdatedPlayer";
        private const ulong SteamId = 76561198000000002UL;

        public WhenAnExistingUserLogsInAgain()
        {
            _validatorMock
                .Setup(v => v.ValidateAsync(Ticket))
                .ReturnsAsync(new AuthTicketValidationResult(SteamId, SteamId, false, false));

            var existingUser = new User(SteamId, "OldNickname");
            _userRepositoryMock
                .Setup(r => r.GetBySteamIdAsync(SteamId, CancellationToken.None))
                .ReturnsAsync(existingUser);

            var session = new PlayerSession(SteamId, TimeSpan.FromDays(15));
            _sessionServiceMock
                .Setup(s => s.CreateAsync(SteamId, TimeSpan.FromDays(15)))
                .ReturnsAsync(session);

            _sut = new LoginUseCase(_validatorMock.Object, _sessionServiceMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task It_UpdatesNicknameAndLastLogin()
        {
            await _sut.ExecuteAsync(new LoginCommand(Ticket, NewNickname));

            _userRepositoryMock.Verify(r => r.UpdateAsync(
                SteamId,
                NewNickname,
                It.IsAny<DateTime>(),
                CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task It_DoesNotCreateANewUser()
        {
            await _sut.ExecuteAsync(new LoginCommand(Ticket, NewNickname));

            _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task It_ReturnsASessionId()
        {
            var result = await _sut.ExecuteAsync(new LoginCommand(Ticket, NewNickname));

            Assert.NotEmpty(result.SessionId);
        }
    }
}
