using Gamism.SDK.Core.Network;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PushAndPull.Domain.Auth.Dto.Request;
using PushAndPull.Domain.Auth.Dto.Response;
using PushAndPull.Domain.Auth.Service.Interface;
using PushAndPull.Global.Security;

namespace PushAndPull.Domain.Auth.Controller;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly ILogoutService _logoutService;

    public AuthController(
        ILoginService loginService,
        ILogoutService logoutService
        )
    {
        _loginService = loginService;
        _logoutService = logoutService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<CommonApiResponse<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct
        )
    {
        var result = await _loginService.ExecuteAsync(new LoginCommand(
            request.SteamTicket,
            request.Nickname
        ), ct);

        return CommonApiResponse.Success("로그인되었습니다.", new LoginResponse(result.SessionId));
    }

    [SessionAuthorize]
    [HttpPost("logout")]
    public async Task<CommonApiResponse> Logout(CancellationToken ct)
    {
        var sessionId = User.GetSessionId();

        await _logoutService.ExecuteAsync(new LogoutCommand(sessionId), ct);

        return CommonApiResponse.Success("로그아웃되었습니다.");
    }
}
