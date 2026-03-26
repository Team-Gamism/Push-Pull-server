using PushAndPull.Domain.Auth.Service.Interface;

namespace PushAndPull.Domain.Auth.Service;

public class LogoutUseCase : ILogoutUseCase
{
    private readonly ISessionService _sessionService;

    public LogoutUseCase(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task ExecuteAsync(LogoutCommand request)
    {
        await _sessionService.DeleteAsync(request.SessionId);
    }
}
