using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PushAndPull.Global.Infrastructure;
using Testcontainers.PostgreSql;

namespace PushAndPull.Test.Support;

/// <summary>
/// Starts a single PostgreSQL container shared across the "Postgres" test collection and
/// applies the production EF schema once, so repository tests run against the real engine.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        return new AppDbContext(options);
    }
}

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;

/// <summary>
/// Base for repository tests: each test gets a fresh context wrapped in a transaction that is
/// rolled back on dispose, keeping tests isolated without recreating the schema every time.
/// </summary>
[Trait("Category", "Integration")]
public abstract class RepositoryTestBase : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private IDbContextTransaction _transaction = null!;

    protected AppDbContext Db { get; private set; } = null!;

    protected RepositoryTestBase(PostgresFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        Db = _fixture.CreateContext();
        _transaction = await Db.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.DisposeAsync();
        await Db.DisposeAsync();
    }
}
