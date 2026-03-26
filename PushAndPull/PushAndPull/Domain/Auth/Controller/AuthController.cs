using Gamism.SDK.Core.Network;
using Microsoft.AspNetCore.Mvc;
using PushAndPull.Domain.Auth.Dto.Request;
using PushAndPull.Domain.Auth.Dto.Response;
using PushAndPull.Domain.Auth.Service.Interface;
using PushAndPull.Global.Security;

namespace PushAndPull.Domain.Auth.Controller;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILoginUseCase _loginUseCase;
    private readonly ILogoutUseCase _logoutUseCase;

    public AuthController(
        ILoginUseCase loginUseCase,
        ILogoutUseCase logoutUseCase
        )
    {
        _loginUseCase = loginUseCase;
        _logoutUseCase = logoutUseCase;
    }

    [HttpPost("login")]
    public async Task<LoginResponse> Login(
        [FromBody] LoginRequest request
        )
    {
        var result = await _loginUseCase.ExecuteAsync(new LoginCommand(
            request.SteamTicket,
            request.Nickname
            )
        );

        return new LoginResponse(result.SessionId);
    }

    [SessionAuthorize]
    [HttpPost("logout")]
    public async Task Logout()
    {
        var sessionId = User.GetSessionId();

        await _logoutUseCase.ExecuteAsync(
            new LogoutCommand(sessionId)
        );
    }
}
