using Moq;
using Server.Application.Port.Output;
using Server.Application.Port.Output.Persistence;
using Server.Application.UseCase.Auth;
using Server.Domain.Entity;
using Server.Application.Port.Input;
using Server.Domain.Exception.Auth;

namespace Tests.UseCase.Auth;

public class LoginUseCaseTests
{
    public class WhenAValidTicketIsProvided
    {
        private readonly Mock<IAuthTicketValidator> _validatorMock = new();
        private readonly Mock<ISessionService> _sessionServiceMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly LoginUseCase _sut;

        private const string Ticket = "valid-ticket";
        private const string Nickname = "TestPlayer";
        private const ulong SteamId = 76561198000000001UL;

        public WhenAValidTicketIsProvided()
        {
            _validatorMock
                .Setup(v => v.ValidateAsync(Ticket))
                .ReturnsAsync(new AuthTicketValidationResult(SteamId, SteamId, false, false));

            var session = new PlayerSession(SteamId, TimeSpan.FromDays(15));
            _sessionServiceMock
                .Setup(s => s.CreateAsync(SteamId, TimeSpan.FromDays(15)))
                .ReturnsAsync(session);

            _sut = new LoginUseCase(_validatorMock.Object, _sessionServiceMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task It_UpsertUser()
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
        public async Task It_DoesNotCallGetBySteamId()
        {
            await _sut.ExecuteAsync(new LoginCommand(Ticket, Nickname));

            _userRepositoryMock.Verify(r => r.GetBySteamIdAsync(
                It.IsAny<ulong>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task It_DoesNotCallUpdateUser()
        {
            await _sut.ExecuteAsync(new LoginCommand(Ticket, Nickname));

            _userRepositoryMock.Verify(r => r.UpdateAsync(
                It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<DateTime>(), CancellationToken.None), Times.Never);
        }
    }

    public class WhenFamilySharingIsDetected
    {
        private readonly Mock<IAuthTicketValidator> _validatorMock = new();
        private readonly Mock<ISessionService> _sessionServiceMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly LoginUseCase _sut;

        private const string Ticket = "shared-ticket";
        private const string Nickname = "SharedPlayer";
        private const ulong SteamId = 76561198000000003UL;
        private const ulong OwnerSteamId = 76561198000000004UL;

        public WhenFamilySharingIsDetected()
        {
            _validatorMock
                .Setup(v => v.ValidateAsync(Ticket))
                .ReturnsAsync(new AuthTicketValidationResult(SteamId, OwnerSteamId, false, false));

            _sut = new LoginUseCase(_validatorMock.Object, _sessionServiceMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task It_ThrowsFamilySharingNotAllowedException()
        {
            await Assert.ThrowsAsync<FamilySharingNotAllowedException>(
                () => _sut.ExecuteAsync(new LoginCommand(Ticket, Nickname)));
        }

        [Fact]
        public async Task It_DoesNotCreateUser()
        {
            await Assert.ThrowsAsync<FamilySharingNotAllowedException>(
                () => _sut.ExecuteAsync(new LoginCommand(Ticket, Nickname)));

            _userRepositoryMock.Verify(r => r.CreateAsync(
                It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
