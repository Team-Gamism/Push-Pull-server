using Azure.Identity;
using Gamism.SDK.Extensions.AspNetCore;
using PushAndPull.Domain.Auth.Config;
using PushAndPull.Domain.Room.Config;
using PushAndPull.Global.Config;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = new Uri(builder.Configuration["KeyVault:Uri"]!);
builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());

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

var app = builder.Build();

app.UseGamismSdk();
app.MapControllers();
app.Run();
