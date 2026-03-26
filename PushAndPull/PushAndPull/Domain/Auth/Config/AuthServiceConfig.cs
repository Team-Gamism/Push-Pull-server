using PushAndPull.Domain.Auth.Repository;
using PushAndPull.Domain.Auth.Service;
using PushAndPull.Domain.Auth.Service.Interface;
using PushAndPull.Global.Auth;

namespace PushAndPull.Domain.Auth.Config;

public static class AuthServiceConfig
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddHttpClient<IAuthTicketValidator, SteamAuthTicketValidator>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<ILogoutUseCase, LogoutUseCase>();
        return services;
    }
}
