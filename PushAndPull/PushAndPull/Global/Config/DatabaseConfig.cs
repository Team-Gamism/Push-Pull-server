using Microsoft.EntityFrameworkCore;
using PushAndPull.Global.Infrastructure;

namespace PushAndPull.Global.Config;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration[configuration["KeyVault:DbConnectionSecretName"]!]
                               ?? throw new InvalidOperationException("DB 연결 문자열을 Key Vault에서 가져올 수 없습니다.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "room"));
        });

        return services;
    }
}
