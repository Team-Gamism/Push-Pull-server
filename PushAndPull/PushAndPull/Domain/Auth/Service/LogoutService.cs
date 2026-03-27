using PushAndPull.Domain.Auth.Service.Interface;

namespace PushAndPull.Domain.Auth.Service;

public class LogoutService : ILogoutService
{
    private readonly ISessionService _sessionService;

    public LogoutService(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task ExecuteAsync(LogoutCommand request)
    {
        await _sessionService.DeleteAsync(request.SessionId);
    }
}
