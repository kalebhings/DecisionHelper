using DecisionHelper.Data;
using DecisionHelper.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Services;

public class MovieService
{
    private readonly IDbContextFactory<DecisionHelperDbContext>
        _dbContextFactory;

    public MovieService(
        IDbContextFactory<DecisionHelperDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Movie?> AddMovieAsync(
        ulong guildId,
        string title,
        int? releaseYear,
        int addedByPersonId)
    {
        string validatedTitle = InputValidator.MovieTitle(title);

        if (releaseYear is < 1900 or > 2100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(releaseYear),
                "Release year must be between 1900 and 2100.");
        }

        string guildUserId = guildId.ToString();
        string normalizedTitle = NormalizeTitle(validatedTitle);

        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        bool personBelongsToGuild = await db.People.AnyAsync(person =>
            person.Id == addedByPersonId &&
            person.GuildId == guildUserId);

        if (!personBelongsToGuild)
        {
            throw new ArgumentException(
                "The person does not belong to this server.",
                nameof(addedByPersonId));
        }

        bool alreadyExists = await db.Movies
            .AnyAsync(movie =>
                movie.GuildId == guildUserId &&
                movie.NormalizedTitle == normalizedTitle &&
                movie.ReleaseYear == releaseYear);

        if (alreadyExists)
        {
            return null;
        }

        var movie = new Movie
        {
            GuildId = guildUserId,
            Title = validatedTitle,
            NormalizedTitle = normalizedTitle,
            ReleaseYear = releaseYear,
            AddedByPersonId = addedByPersonId,
            AddedAtUtc = DateTime.UtcNow
        };

        db.Movies.Add(movie);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            return null;
        }

        return movie;
    }

    public async Task<IReadOnlyList<Movie>> GetMoviesAsync(
        ulong guildId,
        MovieFilter filter)
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        IQueryable<Movie> query = db.Movies
            .AsNoTracking()
            .Include(movie => movie.AddedBy)
            .Where(movie => movie.GuildId == guildId.ToString());

        query = ApplyFilter(query, filter);

        return await query
            .OrderBy(movie => movie.Title)
            .ToListAsync();
    }

    public async Task<Movie?> GetRandomMovieAsync(
        ulong guildId,
        MovieFilter filter)
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        IQueryable<Movie> query = db.Movies
            .AsNoTracking()
            .Include(movie => movie.AddedBy)
            .Where(movie => movie.GuildId == guildId.ToString());

        query = ApplyFilter(query, filter);

        int count = await query.CountAsync();

        if (count == 0)
        {
            return null;
        }

        int index = Random.Shared.Next(count);

        return await query
            .OrderBy(movie => movie.Id)
            .Skip(index)
            .FirstOrDefaultAsync();
    }

    private static string NormalizeTitle(string title)
    {
        return title
            .Trim()
            .ToUpperInvariant();
    }

    public async Task<MovieWatchResult> MarkMovieWatchedAsync(
        ulong guildId,
        string title,
        int? releaseYear,
        int personId)
    {
        string normalizedTitle = NormalizeTitle(
            InputValidator.MovieTitle(title));

        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        bool personBelongsToGuild = await db.People.AnyAsync(person =>
            person.Id == personId &&
            person.GuildId == guildId.ToString());

        if (!personBelongsToGuild)
        {
            throw new ArgumentException(
                "The person does not belong to this server.",
                nameof(personId));
        }

        IQueryable<Movie> movieQuery = db.Movies.Where(movie =>
            movie.GuildId == guildId.ToString() &&
            movie.NormalizedTitle == normalizedTitle);

        if (releaseYear.HasValue)
        {
            movieQuery = movieQuery.Where(movie =>
                movie.ReleaseYear == releaseYear);
        }

        List<Movie> matches = await movieQuery
            .Take(2)
            .ToListAsync();

        if (matches.Count > 1)
        {
            return MovieWatchResult.AmbiguousMovie;
        }

        Movie? movie = matches.SingleOrDefault();

        if (movie is null)
        {
            return MovieWatchResult.MovieNotFound;
        }

        MovieWatchStatus? status =
            await db.MovieWatchStatuses
                .SingleOrDefaultAsync(status =>
                    status.MovieId == movie.Id &&
                    status.PersonId == personId);

        if (status is null)
        {
            status = new MovieWatchStatus
            {
                MovieId = movie.Id,
                PersonId = personId,
                HasSeen = true,
                WatchedAtUtc = DateTime.UtcNow
            };

            db.MovieWatchStatuses.Add(status);
        }
        else if (status.HasSeen)
        {
            return MovieWatchResult.AlreadyWatched;
        }
        else
        {
            status.HasSeen = true;
            status.WatchedAtUtc = DateTime.UtcNow;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            return MovieWatchResult.AlreadyWatched;
        }

        return MovieWatchResult.MarkedWatched;
    }

    private static IQueryable<Movie> ApplyFilter(
        IQueryable<Movie> query,
        MovieFilter filter)
    {
        if (filter.AddedByPersonIds is { Count: > 0 })
        {
            IReadOnlyCollection<int> personIds = filter.AddedByPersonIds;

            query = query.Where(movie =>
                personIds.Contains(movie.AddedByPersonId));
        }

        if (filter.WatchStatusPersonId.HasValue)
        {
            int personId =
                filter.WatchStatusPersonId.Value;

            switch (filter.WatchStatus)
            {
                case WatchFilter.Watched:
                    query = query.Where(movie =>
                        movie.WatchStatuses.Any(status =>
                            status.PersonId == personId &&
                            status.HasSeen));
                    break;

                case WatchFilter.Unwatched:
                    query = query.Where(movie =>
                        !movie.WatchStatuses.Any(status =>
                            status.PersonId == personId &&
                            status.HasSeen));
                    break;
            }
        }

        return query;
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqliteException
        {
            SqliteExtendedErrorCode: 1555 or 2067
        };
    }
}
