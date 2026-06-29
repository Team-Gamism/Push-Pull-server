using System.Net;
using Gamism.SDK.Core.Network;
using Gamism.SDK.Extensions.AspNetCore.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PushAndPull.Domain.Auth.Dto.Request;
using PushAndPull.Domain.Auth.Dto.Response;
using PushAndPull.Domain.Auth.Exception;
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

    [ApiDoc("로그인", "스팀 인증 티켓을 검증하고 세션을 발급한다.")]
    [ApiError(typeof(InvalidNicknameException), "닉네임이 올바르지 않습니다.")]
    [ApiError(typeof(InvalidTicketException), "스팀 인증 티켓이 유효하지 않습니다.")]
    [ApiError(HttpStatusCode.Forbidden, "차단되었거나 패밀리 공유 계정은 이용할 수 없습니다.")]
    [ApiError(HttpStatusCode.ServiceUnavailable, "스팀 인증 서버에 일시적으로 연결할 수 없습니다.")]
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

    [ApiDoc("로그아웃", "현재 세션을 만료시킨다.")]
    [ApiError(HttpStatusCode.Unauthorized, "세션이 유효하지 않습니다.")]
    [SessionAuthorize]
    [HttpPost("logout")]
    public async Task<CommonApiResponse> Logout(CancellationToken ct)
    {
        var sessionId = User.GetSessionId();

        await _logoutService.ExecuteAsync(new LogoutCommand(sessionId), ct);

        return CommonApiResponse.Success("로그아웃되었습니다.");
    }
}
