using PushAndPull.Domain.Room.Repository;
using PushAndPull.Domain.Room.Repository.Interface;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Config;

public static class RoomServiceConfig
{
    public static IServiceCollection AddRoomServices(this IServiceCollection services)
    {
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<ICreateRoomService, CreateRoomService>();
        services.AddScoped<IGetRoomService, GetRoomService>();
        services.AddScoped<IGetAllRoomService, GetAllRoomService>();
        services.AddScoped<IJoinRoomService, JoinRoomService>();
        return services;
    }
}
