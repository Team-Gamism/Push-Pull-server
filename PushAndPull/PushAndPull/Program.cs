using Gamism.SDK.Extensions.AspNetCore;
using Microsoft.EntityFrameworkCore;
using PushAndPull.Domain.Auth.Config;
using PushAndPull.Domain.Room.Config;
using PushAndPull.Global.Config;
using PushAndPull.Global.Infrastructure;

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseRateLimiter();
app.UseGamismSdk();
app.MapControllers();
app.Run();
