using Moq;
using PushAndPull.Domain.Auth.Service;
using PushAndPull.Domain.Auth.Service.Interface;

namespace Tests.Service.Auth;

public class LogoutUseCaseTests
{
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
