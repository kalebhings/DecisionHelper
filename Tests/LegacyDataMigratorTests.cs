using DecisionHelper.Data;
using DecisionHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Tests;

public class LegacyDataMigratorTests
{
    [Fact]
    public async Task LegacyGraphIsCopiedToEveryConfiguredGuildOnce()
    {
        await using SqliteDbContextFactory factory =
            await SqliteDbContextFactory.CreateAsync();

        await using (DecisionHelperDbContext db = factory.CreateDbContext())
        {
            var person = new Person
            {
                GuildId = string.Empty,
                DiscordUserId = "10",
                Nickname = "Person"
            };
            db.People.Add(person);
            await db.SaveChangesAsync();

            var movie = new Movie
            {
                GuildId = string.Empty,
                Title = "Movie",
                NormalizedTitle = "MOVIE",
                AddedByPersonId = person.Id,
                AddedAtUtc = DateTime.UtcNow
            };
            db.Movies.Add(movie);
            await db.SaveChangesAsync();

            db.MovieWatchStatuses.Add(new MovieWatchStatus
            {
                MovieId = movie.Id,
                PersonId = person.Id,
                HasSeen = true,
                WatchedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var migrator = new LegacyDataMigrator(factory);
        await migrator.MigrateAsync([100, 200]);
        await migrator.MigrateAsync([100, 200]);

        await using DecisionHelperDbContext verification =
            factory.CreateDbContext();

        Assert.Equal(2, await verification.People.CountAsync());
        Assert.Equal(2, await verification.Movies.CountAsync());
        Assert.Equal(2, await verification.MovieWatchStatuses.CountAsync());
        Assert.Empty(await verification.People
            .Where(person => person.GuildId == string.Empty)
            .ToListAsync());
        Assert.Equal(
            ["100", "200"],
            await verification.Movies
                .OrderBy(movie => movie.GuildId)
                .Select(movie => movie.GuildId)
                .ToListAsync());
    }
}
