using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.Service;

namespace Server.Application.UseCase.Auth;

public class LoginUseCase : ILoginUseCase
{
    private readonly IAuthTicketValidator _validator;
    private readonly SessionService _sessionService;

    public LoginUseCase(
        IAuthTicketValidator validator,
        SessionService sessionService
        )
    {
        _validator = validator;
        _sessionService = sessionService;
    }
    
    public async Task<LoginResult> ExecuteAsync(LoginCommand request)
    {
        var authResult = await _validator.ValidateAsync(request.Ticket);

        var session = await _sessionService.CreateAsync(
            authResult.SteamId, TimeSpan.FromDays(15)
            );
        
        return new LoginResult(session.SessionId.ToString());
    }
}