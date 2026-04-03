using Moq;
using PushAndPull.Domain.Auth.Service;
using PushAndPull.Domain.Auth.Service.Interface;

namespace PushAndPull.Test.Service.Auth;

public class LogoutServiceTests
{
    public class WhenAUserLogsOutWithAValidSession
    {
        private readonly Mock<ISessionService> _sessionServiceMock = new();
        private readonly LogoutService _sut;

        private const string SessionId = "session-xyz789";

        public WhenAUserLogsOutWithAValidSession()
        {
            _sessionServiceMock
                .Setup(s => s.DeleteAsync(SessionId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _sut = new LogoutService(_sessionServiceMock.Object);
        }

        [Fact]
        public async Task It_DeletesTheSession()
        {
            await _sut.ExecuteAsync(new LogoutCommand(SessionId));

            _sessionServiceMock.Verify(s => s.DeleteAsync(SessionId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
