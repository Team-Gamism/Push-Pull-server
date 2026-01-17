using Server.Application.Port.Input;
using Server.Application.Port.Output;
using Server.Application.Service;
using Server.Application.UseCase.Auth;
using Server.Infrastructure.Auth;
using Server.Infrastructure.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();