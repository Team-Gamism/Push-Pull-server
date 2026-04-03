using PushAndPull.Domain.Auth.Entity;
using PushAndPull.Domain.Auth.Exception;
using PushAndPull.Domain.Auth.Repository.Interface;
using PushAndPull.Domain.Auth.Service.Interface;
using PushAndPull.Global.Auth;

namespace PushAndPull.Domain.Auth.Service;

public class LoginService : ILoginService
{
    private readonly IAuthTicketValidator _validator;
    private readonly ISessionService _sessionService;
    private readonly IUserRepository _userRepository;

    public LoginService(
        IAuthTicketValidator validator,
        ISessionService sessionService,
        IUserRepository userRepository
        )
    {
        _validator = validator;
        _sessionService = sessionService;
        _userRepository = userRepository;
    }

    public async Task<LoginResult> ExecuteAsync(LoginCommand request, CancellationToken ct = default)
    {
        var authResult = await _validator.ValidateAsync(request.Ticket);

        if (authResult.IsFamilySharing)
            throw new FamilySharingNotAllowedException(authResult.SteamId);

        var existingUser = await _userRepository.GetBySteamIdAsync(authResult.SteamId, ct);

        if (existingUser is null)
        {
            var user = new User(authResult.SteamId, request.Nickname);
            await _userRepository.CreateAsync(user, ct);
        }
        else
        {
            await _userRepository.UpdateAsync(authResult.SteamId, request.Nickname, DateTime.UtcNow, ct);
        }

        var session = await _sessionService.CreateAsync(
            authResult.SteamId, TimeSpan.FromDays(15)
        );

        return new LoginResult(session.SessionId);
    }
}
