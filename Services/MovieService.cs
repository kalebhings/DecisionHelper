using DecisionHelper.Data;
using DecisionHelper.Models;
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
        string title,
        int? releaseYear,
        int addedByPersonId)
    {
        string normalizedTitle = NormalizeTitle(title);

        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new ArgumentException(
                "Movie title cannot be empty.",
                nameof(title));
        }

        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        bool alreadyExists = await db.Movies
            .AnyAsync(movie =>
                movie.NormalizedTitle == normalizedTitle &&
                movie.ReleaseYear == releaseYear);

        if (alreadyExists)
        {
            return null;
        }

        var movie = new Movie
        {
            Title = title.Trim(),
            NormalizedTitle = normalizedTitle,
            ReleaseYear = releaseYear,
            AddedByPersonId = addedByPersonId,
            AddedAtUtc = DateTime.UtcNow
        };

        db.Movies.Add(movie);

        await db.SaveChangesAsync();

        return movie;
    }

    public async Task<IReadOnlyList<Movie>> GetAllMoviesAsync()
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        return await db.Movies
            .AsNoTracking()
            .Include(movie => movie.AddedBy)
            .OrderBy(movie => movie.Title)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Movie>> GetMoviesByPersonAsync(
        int personId)
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        return await db.Movies
            .AsNoTracking()
            .Include(movie => movie.AddedBy)
            .Where(movie =>
                movie.AddedByPersonId == personId)
            .OrderBy(movie => movie.Title)
            .ToListAsync();
    }

    public async Task<Movie?> GetRandomMovieAsync()
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        int count = await db.Movies.CountAsync();

        if (count == 0)
        {
            return null;
        }

        int randomIndex = Random.Shared.Next(count);

        return await db.Movies
            .AsNoTracking()
            .Include(movie => movie.AddedBy)
            .Skip(randomIndex)
            .FirstOrDefaultAsync();
    }

    public async Task<Movie?> GetRandomMovieByPersonAsync(
        int personId)
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        var movies = db.Movies
            .AsNoTracking()
            .Include(movie => movie.AddedBy)
            .Where(movie =>
                movie.AddedByPersonId == personId);

        int count = await movies.CountAsync();

        if (count == 0)
        {
            return null;
        }

        int randomIndex = Random.Shared.Next(count);

        return await movies
            .Skip(randomIndex)
            .FirstOrDefaultAsync();
    }

    private static string NormalizeTitle(string title)
    {
        return title
            .Trim()
            .ToUpperInvariant();
    }

public async Task<MovieWatchResult> MarkMovieWatchedAsync(
    string title,
    int personId)
{
    string normalizedTitle = NormalizeTitle(title);

    await using var db =
        await _dbContextFactory.CreateDbContextAsync();

    Movie? movie = await db.Movies
        .SingleOrDefaultAsync(movie =>
            movie.NormalizedTitle == normalizedTitle);

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

    await db.SaveChangesAsync();

    return MovieWatchResult.MarkedWatched;
    }
}