using Server.Application.Port.Input;
using Server.Application.Port.Output;

namespace Server.Application.UseCase.Auth;

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