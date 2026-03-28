namespace PushAndPull.Global.Config;

public static class RedisConfig
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"]
                ?? throw new InvalidOperationException("Redis:ConnectionString is not configured.");
        });

        return services;
    }
}
