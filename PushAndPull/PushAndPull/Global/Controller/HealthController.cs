using Microsoft.AspNetCore.Mvc;

namespace PushAndPull.Global.Controller;

[ApiController]
[Route("api/v1/health")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult CheckHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Seoul")
        });
    }
}
