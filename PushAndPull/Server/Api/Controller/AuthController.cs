using Microsoft.AspNetCore.Mvc;
using Server.Api.Dto.Request;
using Server.Api.Dto.Response;
using Server.Application.Port.Input;

namespace Server.Api.Controller
{
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
        
        [HttpPost]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] LoginRequest request
            )
        {
            var result = await _loginUseCase.ExecuteAsync(new LoginCommand(
                request.SteamTicket
                )
            );
            
            var response = new LoginResponse(result.SessionId);
            
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Logout(
            [FromHeader] string sessionId
            )
        {
            await _logoutUseCase.ExecuteAsync(new LogoutCommand(sessionId));
            return NoContent();
        }
    }
}
