using DecisionHelper.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Tests;

internal sealed class SqliteDbContextFactory :
    IDbContextFactory<DecisionHelperDbContext>, IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DecisionHelperDbContext> _options;

    private SqliteDbContextFactory(SqliteConnection connection)
    {
        _connection = connection;
        _options = new DbContextOptionsBuilder<DecisionHelperDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    public static async Task<SqliteDbContextFactory> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var factory = new SqliteDbContextFactory(connection);
        await using DecisionHelperDbContext db = factory.CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        return factory;
    }

    public DecisionHelperDbContext CreateDbContext() => new(_options);

    public ValueTask<DecisionHelperDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(CreateDbContext());

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
