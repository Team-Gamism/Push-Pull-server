using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PushAndPull.Global.Infrastructure;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=pushpull_design;Username=postgres",
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public"))
            .Options;

        return new AppDbContext(options);
    }
}
