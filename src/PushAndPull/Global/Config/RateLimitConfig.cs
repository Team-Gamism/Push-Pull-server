using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace PushAndPull.Global.Config;

public static class RateLimitConfig
{
    public static IServiceCollection AddRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // 레이트리밋 미들웨어는 세션 인증 필터보다 먼저 실행되므로 claims를 쓸 수 없고,
            // 클라이언트가 조작 가능한 헤더(Session-Id)는 우회 가능하므로 IP를 파티션 키로 사용한다.
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
                    partitionKey: GetIpKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddPolicy("join_room", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetIpKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // 인증 없이 열려 있는 방 조회 엔드포인트의 열거/스크래핑을 제한한다.
            options.AddPolicy("read_room", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetIpKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
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
}
