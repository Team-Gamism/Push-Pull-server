using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;

namespace PushAndPull.Global.Config;

public static class RateLimitConfig
{
    public static IServiceCollection AddRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("login", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(1);
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.ContentType = "application/json";
                var body = JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "요청이 너무 많습니다. 잠시 후 다시 시도해주세요.",
                    data = (object?)null
                });
                await context.HttpContext.Response.WriteAsync(body, token);
            };
        });

        return services;
    }
}
