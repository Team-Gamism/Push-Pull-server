using StackExchange.Redis;

namespace PushAndPull.Global.Config;

public static class RedisConfig
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["Redis:ConnectionString"]
            ?? throw new InvalidOperationException("Redis:ConnectionString is not configured.");

        services.AddStackExchangeRedisCache(options =>
        {
            var configOptions = ConfigurationOptions.Parse(connectionString);
            configOptions.AbortOnConnectFail = false;
            options.ConfigurationOptions = configOptions;
        });

        return services;
    }
}
