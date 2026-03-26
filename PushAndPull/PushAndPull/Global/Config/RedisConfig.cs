using StackExchange.Redis;

namespace PushAndPull.Global.Config;

public static class RedisConfig
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var connStr = configuration["Redis:ConnectionString"]
                          ?? throw new InvalidOperationException();
            return ConnectionMultiplexer.Connect(connStr);
        });

        return services;
    }
}
