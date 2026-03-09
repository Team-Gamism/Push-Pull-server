using Moq;
using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.UseCase.Auth;

namespace Tests.UseCase.Auth;

// Describe: LogoutUseCase
public class LogoutUseCaseTests
{
    // Context: When a user logs out with a valid session
    public class WhenAUserLogsOutWithAValidSession
    {
        private readonly Mock<ISessionService> _sessionServiceMock = new();
        private readonly LogoutUseCase _sut;

        private const string SessionId = "session-xyz789";

        public WhenAUserLogsOutWithAValidSession()
        {
            _sessionServiceMock
                .Setup(s => s.DeleteAsync(SessionId))
                .Returns(Task.CompletedTask);

            _sut = new LogoutUseCase(_sessionServiceMock.Object);
        }

        [Fact]
        public async Task It_DeletesTheSession()
        {
            await _sut.ExecuteAsync(new LogoutCommand(SessionId));

            _sessionServiceMock.Verify(s => s.DeleteAsync(SessionId), Times.Once);
        }
    }
}
