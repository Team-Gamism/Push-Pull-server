using System.Net;
using Gamism.SDK.Core.Network;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PushAndPull.Domain.Auth.Exception;

namespace PushAndPull.Global.Filter;

public class SteamApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<SteamApiExceptionFilter> _logger;

    public SteamApiExceptionFilter(ILogger<SteamApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not SteamApiException ex)
            return;

        _logger.LogWarning(ex, "Steam auth upstream failure");

        context.Result = new ObjectResult(
            CommonApiResponse.Error(
                "스팀 인증 서버에 일시적으로 연결할 수 없습니다. 잠시 후 다시 시도해 주세요.",
                HttpStatusCode.ServiceUnavailable))
        {
            StatusCode = StatusCodes.Status503ServiceUnavailable
        };
        context.ExceptionHandled = true;
    }
}
