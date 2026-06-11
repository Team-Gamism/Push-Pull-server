using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace PushAndPull.Global.Config;

public static class RateLimitConfig
{
    public static IServiceCollection AddRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // 레이트리밋 미들웨어는 세션 인증 필터보다 먼저 실행되므로
            // claims 대신 클라이언트 IP 또는 Session-Id 헤더를 파티션 키로 사용한다.
            options.AddPolicy("login", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetIpKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddPolicy("create_room", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetSessionOrIpKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddPolicy("join_room", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetSessionOrIpKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Too Many Requests", token);
            };
        });

        return services;
    }

    internal static string GetIpKey(HttpContext httpContext)
        => $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    internal static string GetSessionOrIpKey(HttpContext httpContext)
        => httpContext.Request.Headers.TryGetValue("Session-Id", out var sessionId)
           && !string.IsNullOrWhiteSpace(sessionId)
            ? $"session:{sessionId}"
            : GetIpKey(httpContext);
}
