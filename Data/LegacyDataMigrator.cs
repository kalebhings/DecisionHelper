using DecisionHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Data;

public sealed class LegacyDataMigrator
{
    private const string LegacyGuildId = "";

    private readonly IDbContextFactory<DecisionHelperDbContext>
        _dbContextFactory;

    public LegacyDataMigrator(
        IDbContextFactory<DecisionHelperDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task MigrateAsync(IReadOnlyList<ulong> serverIds)
    {
        await using DecisionHelperDbContext db =
            await _dbContextFactory.CreateDbContextAsync();

        await using var transaction = await db.Database.BeginTransactionAsync();

        // Acquire SQLite's write lock before reading the legacy snapshot.
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE People SET GuildId = GuildId WHERE GuildId = ''");

        List<Person> people = await db.People
            .AsNoTracking()
            .Where(person => person.GuildId == LegacyGuildId)
            .ToListAsync();

        List<Movie> movies = await db.Movies
            .AsNoTracking()
            .Where(movie => movie.GuildId == LegacyGuildId)
            .ToListAsync();

        List<Tag> tags = await db.Tags
            .AsNoTracking()
            .Where(tag => tag.GuildId == LegacyGuildId)
            .ToListAsync();

        if (people.Count == 0 && movies.Count == 0 && tags.Count == 0)
        {
            await transaction.CommitAsync();
            return;
        }

        int[] movieIds = movies.Select(movie => movie.Id).ToArray();
        int[] tagIds = tags.Select(tag => tag.Id).ToArray();

        List<MovieWatchStatus> statuses = await db.MovieWatchStatuses
            .AsNoTracking()
            .Where(status => movieIds.Contains(status.MovieId))
            .ToListAsync();

        List<MovieTag> movieTags = await db.MovieTags
            .AsNoTracking()
            .Where(movieTag =>
                movieIds.Contains(movieTag.MovieId) &&
                tagIds.Contains(movieTag.TagId))
            .ToListAsync();

        string firstGuildId = serverIds[0].ToString();

        await db.People
            .Where(person => person.GuildId == LegacyGuildId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(person => person.GuildId, firstGuildId));

        await db.Movies
            .Where(movie => movie.GuildId == LegacyGuildId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(movie => movie.GuildId, firstGuildId));

        await db.Tags
            .Where(tag => tag.GuildId == LegacyGuildId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(tag => tag.GuildId, firstGuildId));

        foreach (ulong serverId in serverIds.Skip(1))
        {
            await CloneGuildDataAsync(
                db,
                serverId.ToString(),
                people,
                movies,
                tags,
                statuses,
                movieTags);
        }

        await transaction.CommitAsync();
    }

    private static async Task CloneGuildDataAsync(
        DecisionHelperDbContext db,
        string guildId,
        IReadOnlyCollection<Person> people,
        IReadOnlyCollection<Movie> movies,
        IReadOnlyCollection<Tag> tags,
        IReadOnlyCollection<MovieWatchStatus> statuses,
        IReadOnlyCollection<MovieTag> movieTags)
    {
        var personIds = new Dictionary<int, int>();
        var personClones = new List<(int SourceId, Person Clone)>();

        foreach (Person source in people)
        {
            var clone = new Person
            {
                GuildId = guildId,
                DiscordUserId = source.DiscordUserId,
                Nickname = source.Nickname
            };

            personClones.Add((source.Id, clone));
        }

        db.People.AddRange(personClones.Select(item => item.Clone));
        await db.SaveChangesAsync();

        foreach ((int sourceId, Person clone) in personClones)
        {
            personIds[sourceId] = clone.Id;
        }

        var tagIds = new Dictionary<int, int>();
        var tagClones = new List<(int SourceId, Tag Clone)>();

        foreach (Tag source in tags)
        {
            var clone = new Tag
            {
                GuildId = guildId,
                Name = source.Name,
                NormalizedName = source.NormalizedName,
                Kind = source.Kind
            };

            tagClones.Add((source.Id, clone));
        }

        db.Tags.AddRange(tagClones.Select(item => item.Clone));
        await db.SaveChangesAsync();

        foreach ((int sourceId, Tag clone) in tagClones)
        {
            tagIds[sourceId] = clone.Id;
        }

        var movieIds = new Dictionary<int, int>();
        var movieClones = new List<(int SourceId, Movie Clone)>();

        foreach (Movie source in movies)
        {
            var clone = new Movie
            {
                GuildId = guildId,
                Title = source.Title,
                NormalizedTitle = source.NormalizedTitle,
                ReleaseYear = source.ReleaseYear,
                AddedByPersonId = personIds[source.AddedByPersonId],
                AddedAtUtc = source.AddedAtUtc
            };

            movieClones.Add((source.Id, clone));
        }

        db.Movies.AddRange(movieClones.Select(item => item.Clone));
        await db.SaveChangesAsync();

        foreach ((int sourceId, Movie clone) in movieClones)
        {
            movieIds[sourceId] = clone.Id;
        }

        db.MovieWatchStatuses.AddRange(statuses.Select(source =>
            new MovieWatchStatus
            {
                MovieId = movieIds[source.MovieId],
                PersonId = personIds[source.PersonId],
                HasSeen = source.HasSeen,
                WatchedAtUtc = source.WatchedAtUtc
            }));

        db.MovieTags.AddRange(movieTags.Select(source =>
            new MovieTag
            {
                MovieId = movieIds[source.MovieId],
                TagId = tagIds[source.TagId]
            }));

        await db.SaveChangesAsync();
    }
}
