using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.Port.Output.Persistence;
using Server.Application.Service;
using Server.Application.UseCase.Auth;
using Server.Application.UseCase.Room;
using Server.Domain.Exception.Auth;
using Server.Infrastructure.Auth;
using Server.Infrastructure.Cache;
using Server.Infrastructure.Persistence.DbContext;
using Server.Infrastructure.Persistence.Repository;
using Server.Infrastructure.Service;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = new Uri(builder.Configuration["KeyVault:Uri"]!);
builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());

var connectionString = builder.Configuration[builder.Configuration["KeyVault:DbConnectionSecretName"]!]
                       ?? throw new InvalidOperationException("DB 연결 문자열을 Key Vault에서 가져올 수 없습니다.");

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "room"));
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{   
    var connStr = builder.Configuration["Redis:ConnectionString"]
                  ?? throw new InvalidOperationException();
    return ConnectionMultiplexer.Connect(connStr);
});

builder.Services.AddScoped<ICacheStore, CacheStore>();

builder.Services.AddHttpClient<IAuthTicketValidator, SteamAuthTicketValidator>();
builder.Services.AddScoped<ISessionService, SessionService>();

builder.Services.AddScoped<ILoginUseCase, LoginUseCase>();
builder.Services.AddScoped<ILogoutUseCase, LogoutUseCase>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

builder.Services.AddSingleton<IRoomCodeGenerator, RoomCodeGenerator>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<ICreateRoomUseCase, CreateRoomUseCase>();
builder.Services.AddScoped<IGetRoomUseCase, GetRoomUseCase>();
builder.Services.AddScoped<IGetAllRoomUseCase, GetAllRoomUseCase>();
builder.Services.AddScoped<IJoinRoomUseCase, JoinRoomUseCase>();

var app = builder.Build();

app.UseExceptionHandler(errApp => errApp.Run(ctx =>
{
    var feature = ctx.Features.Get<IExceptionHandlerFeature>();
    ctx.Response.StatusCode = feature?.Error switch
    {
        FamilySharingNotAllowedException => StatusCodes.Status403Forbidden,
        VacBannedException               => StatusCodes.Status403Forbidden,
        PublisherBannedException         => StatusCodes.Status403Forbidden,
        InvalidTicketException           => StatusCodes.Status401Unauthorized,
        _                                => StatusCodes.Status500InternalServerError
    };
    return Task.CompletedTask;
}));

app.MapControllers();
app.Run();