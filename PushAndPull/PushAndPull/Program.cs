using Gamism.SDK.Extensions.AspNetCore;
using PushAndPull.Domain.Auth.Config;
using PushAndPull.Domain.Room.Config;
using PushAndPull.Global.Config;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddGamismSdk(options =>
{
    options.Swagger.Title = "Push & Pull API";
    options.Logging.NotLoggingUrls = ["/swagger/**", "/health"];
    options.Response.NotWrappingUrls = ["/swagger/**", "/health"];
});

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddGlobalServices();
builder.Services.AddAuthServices();
builder.Services.AddRoomServices();
builder.Services.AddRateLimit();

var app = builder.Build();

app.UseRateLimiter();
app.UseGamismSdk();
app.MapControllers();
app.Run();
