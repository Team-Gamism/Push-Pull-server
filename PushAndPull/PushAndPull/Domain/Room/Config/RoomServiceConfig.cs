using PushAndPull.Domain.Room.Repository;
using PushAndPull.Domain.Room.Service;
using PushAndPull.Domain.Room.Service.Interface;

namespace PushAndPull.Domain.Room.Config;

public static class RoomServiceConfig
{
    public static IServiceCollection AddRoomServices(this IServiceCollection services)
    {
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<ICreateRoomUseCase, CreateRoomUseCase>();
        services.AddScoped<IGetRoomUseCase, GetRoomUseCase>();
        services.AddScoped<IGetAllRoomUseCase, GetAllRoomUseCase>();
        services.AddScoped<IJoinRoomUseCase, JoinRoomUseCase>();
        return services;
    }
}
