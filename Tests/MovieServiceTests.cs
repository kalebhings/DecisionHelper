using DecisionHelper.Models;
using DecisionHelper.Services;

namespace DecisionHelper.Tests;

public class MovieServiceTests
{
    [Fact]
    public async Task QueriesAreGuildScopedAndSqlLikeTitlesRemainData()
    {
        await using SqliteDbContextFactory factory =
            await SqliteDbContextFactory.CreateAsync();
        var people = new PersonService(factory);
        var movies = new MovieService(factory);

        Person firstPerson = await people.GetOrCreatePersonAsync(1, 10, "First");
        Person secondPerson = await people.GetOrCreatePersonAsync(2, 10, "Second");
        const string title = "Robert'); DROP TABLE Movies;--";

        await movies.AddMovieAsync(1, title, null, firstPerson.Id);
        await movies.AddMovieAsync(1, "Second", null, firstPerson.Id);
        Person otherFirstGuildPerson =
            await people.GetOrCreatePersonAsync(1, 11, "Other Person");
        await movies.AddMovieAsync(1, "Third", null, otherFirstGuildPerson.Id);
        await movies.AddMovieAsync(2, "Other", null, secondPerson.Id);

        IReadOnlyList<Movie> firstGuildMovies =
            await movies.GetMoviesAsync(1, new MovieFilter
            {
                AddedByPersonIds = [firstPerson.Id]
            });

        Assert.Equal(2, firstGuildMovies.Count);
        Assert.Contains(firstGuildMovies, movie => movie.Title == title);
        Assert.DoesNotContain(firstGuildMovies, movie => movie.Title == "Third");
    }

    [Fact]
    public async Task NicknameLookupIsCaseInsensitiveAndGuildScoped()
    {
        await using SqliteDbContextFactory factory =
            await SqliteDbContextFactory.CreateAsync();
        var people = new PersonService(factory);

        Person first = await people.GetOrCreatePersonAsync(1, 10, "Kaleb");
        await people.GetOrCreatePersonAsync(2, 10, "Kaleb");

        IReadOnlyList<int> matches =
            await people.GetPersonIdsByNicknameAsync(1, "kaleb");

        Assert.Equal([first.Id], matches);
    }

    [Fact]
    public async Task DuplicateTitleWithoutYearIsRejected()
    {
        await using SqliteDbContextFactory factory =
            await SqliteDbContextFactory.CreateAsync();
        var people = new PersonService(factory);
        var movies = new MovieService(factory);
        Person person = await people.GetOrCreatePersonAsync(1, 10, "Person");

        Movie? first = await movies.AddMovieAsync(1, "Dune", null, person.Id);
        Movie? duplicate = await movies.AddMovieAsync(1, "dune", null, person.Id);

        Assert.NotNull(first);
        Assert.Null(duplicate);
    }

    [Fact]
    public async Task WatchedTitleRequiresYearWhenMultipleReleasesExist()
    {
        await using SqliteDbContextFactory factory =
            await SqliteDbContextFactory.CreateAsync();
        var people = new PersonService(factory);
        var movies = new MovieService(factory);
        Person person = await people.GetOrCreatePersonAsync(1, 10, "Person");

        await movies.AddMovieAsync(1, "Dune", 1984, person.Id);
        await movies.AddMovieAsync(1, "Dune", 2021, person.Id);

        MovieWatchResult ambiguous = await movies.MarkMovieWatchedAsync(
            1,
            "Dune",
            null,
            person.Id);
        MovieWatchResult selected = await movies.MarkMovieWatchedAsync(
            1,
            "Dune",
            2021,
            person.Id);

        Assert.Equal(MovieWatchResult.AmbiguousMovie, ambiguous);
        Assert.Equal(MovieWatchResult.MarkedWatched, selected);
    }

    [Fact]
    public async Task MovieCannotReferencePersonFromAnotherGuild()
    {
        await using SqliteDbContextFactory factory =
            await SqliteDbContextFactory.CreateAsync();
        var people = new PersonService(factory);
        var movies = new MovieService(factory);
        Person person = await people.GetOrCreatePersonAsync(2, 10, "Person");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            movies.AddMovieAsync(1, "Movie", null, person.Id));
    }
}
