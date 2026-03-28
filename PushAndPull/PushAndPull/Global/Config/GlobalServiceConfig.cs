using PushAndPull.Global.Infrastructure.Cache;
using PushAndPull.Global.Service;

namespace PushAndPull.Global.Config;

public static class GlobalServiceConfig
{
    public static IServiceCollection AddGlobalServices(this IServiceCollection services)
    {
        services.AddSingleton<ICacheStore, CacheStore>();
        services.AddSingleton<IRoomCodeGenerator, RoomCodeGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        return services;
    }
}
