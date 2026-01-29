using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.Port.Output.Persistence;
using Server.Application.Service;
using Server.Application.UseCase.Auth;
using Server.Application.UseCase.Room;
using Server.Infrastructure.Auth;
using Server.Infrastructure.Cache;
using Server.Infrastructure.Persistence.DbContext;
using Server.Infrastructure.Persistence.Repository;
using Server.Infrastructure.Service;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = new Uri(builder.Configuration["KeyVault:Uri"]!);
var secretName = builder.Configuration["KeyVault:DbConnectionSecretName"]!;

var credential = new DefaultAzureCredential();
var secretClient = new SecretClient(keyVaultUri, credential);

KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName);
string connectionString = secret.Value;

builder.Services.AddControllers();

builder.Services.AddDbContext<RoomContext>(options =>
{
    options.UseNpgsql(connectionString);
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

builder.Services.AddScoped<IRoomRepository, RoomRepository>();

builder.Services.AddSingleton<IRoomCodeGenerator, RoomCodeGenerator>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<ICreateRoomUseCase, CreateRoomUseCase>();
builder.Services.AddScoped<IGetRoomUseCase, GetRoomUseCase>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();