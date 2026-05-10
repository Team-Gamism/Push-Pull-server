using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using PushAndPull.Domain.Auth.Repository;
using PushAndPull.Domain.Auth.Repository.Interface;
using PushAndPull.Domain.Auth.Service;
using PushAndPull.Domain.Auth.Service.Interface;
using PushAndPull.Global.Auth;

namespace PushAndPull.Domain.Auth.Config;

public static class AuthServiceConfig
{
    public static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cbOptions = configuration
            .GetSection("Steam:CircuitBreaker")
            .Get<CircuitBreakerOptions>() ?? new CircuitBreakerOptions();

        if (cbOptions.FailureRatio is <= 0 or > 1)
            throw new ArgumentException("FailureRatio must be between 0 (exclusive) and 1 (inclusive).");
        if (cbOptions.SamplingDurationSeconds <= 0)
            throw new ArgumentException("SamplingDurationSeconds must be greater than 0.");
        if (cbOptions.BreakDurationSeconds <= 0)
            throw new ArgumentException("BreakDurationSeconds must be greater than 0.");
        if (cbOptions.RequestTimeoutSeconds <= 0)
            throw new ArgumentException("RequestTimeoutSeconds must be greater than 0.");
        if (cbOptions.MinimumThroughput < 2)
            throw new ArgumentException("MinimumThroughput must be at least 2 (Polly v8 requirement).");

        services
            .AddHttpClient<IAuthTicketValidator, SteamAuthTicketValidator>()
            .AddResilienceHandler("steam-cb", (builder, context) =>
            {
                builder.TimeProvider = context.ServiceProvider.GetService<TimeProvider>() ?? TimeProvider.System;

                var logger = context.ServiceProvider
                    .GetRequiredService<ILogger<SteamAuthTicketValidator>>();

                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = cbOptions.FailureRatio,
                    SamplingDuration = TimeSpan.FromSeconds(cbOptions.SamplingDurationSeconds),
                    MinimumThroughput = cbOptions.MinimumThroughput,
                    BreakDuration = TimeSpan.FromSeconds(cbOptions.BreakDurationSeconds),
                    OnOpened = args =>
                    {
                        logger.LogWarning(
                            "Steam API circuit breaker opened. Break for {Duration}s.",
                            cbOptions.BreakDurationSeconds);
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = _ =>
                    {
                        logger.LogInformation("Steam API circuit breaker closed.");
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = _ =>
                    {
                        logger.LogInformation("Steam API circuit breaker half-opened.");
                        return ValueTask.CompletedTask;
                    }
                });

                builder.AddTimeout(new HttpTimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(cbOptions.RequestTimeoutSeconds)
                });
            });

        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<ILogoutService, LogoutService>();
        return services;
    }
}
