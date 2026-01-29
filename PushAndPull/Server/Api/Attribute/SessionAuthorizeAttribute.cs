using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Server.Application.Port.Output;

namespace Server.Api.Attribute;

public class SessionAuthorizeAttribute : System.Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;

        if (!httpContext.Request.Headers.TryGetValue("Session-Id", out var sessionId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var sessionService =
            httpContext.RequestServices.GetRequiredService<ISessionService>();

        var playerSession = await sessionService.GetAsync(sessionId!);

        if (playerSession == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var claims = new[]
        {
            new Claim(SessionClaim.SessionId, playerSession.SessionId),
            new Claim(SessionClaim.SteamId, playerSession.SteamId.ToString()),
        };

        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "Session")
        );
    }
}

public static class SessionClaim
{
    public const string SessionId = "session_id";
    public const string SteamId = "steam_id";
}