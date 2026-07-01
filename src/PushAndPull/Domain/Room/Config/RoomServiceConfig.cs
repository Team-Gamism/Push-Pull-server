using Microsoft.Extensions.DependencyInjection.Extensions;
using PushAndPull.Domain.Room.Repository;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Config;

public static class RoomServiceConfig
{
    public static IServiceCollection AddRoomServices(
        this IServiceCollection services,
        IConfiguration configuration
        )
    {
        services.Configure<RoomCleanupOptions>(configuration.GetSection("RoomCleanup"));
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<ICreateRoomService, CreateRoomService>();
        services.AddScoped<IGetRoomService, GetRoomService>();
        services.AddScoped<IGetAllRoomService, GetAllRoomService>();
        services.AddScoped<IJoinRoomService, JoinRoomService>();
        services.AddScoped<ILeaveRoomService, LeaveRoomService>();
        services.AddScoped<ICloseRoomService, CloseRoomService>();
        services.AddScoped<IHeartbeatRoomService, HeartbeatRoomService>();
        services.AddScoped<IReconnectRoomService, ReconnectRoomService>();
        services.AddHostedService<StaleRoomCleanupService>();
        return services;
    }
}
