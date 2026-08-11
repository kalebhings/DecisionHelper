using DecisionHelper.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DecisionHelper.Tests;

public class DatabaseMigrationTests
{
    [Fact]
    public async Task GuildMigrationDeduplicatesLegacyNullYearMovies()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<DecisionHelperDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new DecisionHelperDbContext(options);
        IMigrator migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20260809005254_InitialCreate");

        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO People (DiscordUserId, Nickname) VALUES ('10', 'Person')");
        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO Movies
                (Title, NormalizedTitle, ReleaseYear, AddedByPersonId, AddedAtUtc)
            VALUES
                ('Movie', 'MOVIE', NULL, 1, '2026-01-01'),
                ('movie', 'MOVIE', NULL, 1, '2026-01-02');
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO MovieWatchStatuses
                (MovieId, PersonId, HasSeen, WatchedAtUtc)
            VALUES
                (1, 1, 0, NULL),
                (2, 1, 1, '2026-01-03');
            """);

        await migrator.MigrateAsync();

        Assert.Equal(1, await db.Movies.CountAsync());
        Assert.True(await db.Movies.AllAsync(movie => movie.GuildId == string.Empty));
        Assert.True(await db.MovieWatchStatuses
            .Select(status => status.HasSeen)
            .SingleAsync());
        Assert.NotNull(await db.MovieWatchStatuses
            .Select(status => status.WatchedAtUtc)
            .SingleAsync());
    }
}
