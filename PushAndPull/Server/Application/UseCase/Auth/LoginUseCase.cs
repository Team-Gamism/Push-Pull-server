using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.Port.Output.Persistence;
using Server.Domain.Entity;

namespace Server.Application.UseCase.Auth;

public class LoginUseCase : ILoginUseCase
{
    private readonly IAuthTicketValidator _validator;
    private readonly ISessionService _sessionService;
    private readonly IUserRepository _userRepository;  

    public LoginUseCase(
        IAuthTicketValidator validator,
        ISessionService sessionService,
        IUserRepository userRepository
        )
    {
        _validator = validator;
        _sessionService = sessionService;
        _userRepository = userRepository;
    }
    
    public async Task<LoginResult> ExecuteAsync(LoginCommand request)
    {
        var authResult = await _validator.ValidateAsync(request.Ticket);

        var user = await _userRepository.GetBySteamIdAsync(authResult.SteamId);
        if (user == null)
        {
            user = new User(authResult.SteamId, request.Nickname);
            await _userRepository.CreateAsync(user);
        }
        else
        {
            await _userRepository.UpdateAsync(user.SteamId, request.Nickname, DateTime.UtcNow);
        }

        var session = await _sessionService.CreateAsync(
            authResult.SteamId, TimeSpan.FromDays(15)
        );
        
        return new LoginResult(session.SessionId);
    }
}