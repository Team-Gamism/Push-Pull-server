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
        
        public AuthController(ILoginUseCase loginUseCase)
        {
            _loginUseCase = loginUseCase;
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
    }
}
