using DecisionHelper.Models;

namespace DecisionHelper.Services;

public class MovieService
{
  private readonly List<Movie> _movies = [];

  public bool AddMovie(Movie movie)
  {
    ArgumentNullException.ThrowIfNull(movie);

    string normalizedName = movie.Name.Trim();

    if (string.IsNullOrWhiteSpace(normalizedName))
    {
      throw new ArgumentException(
          "Movie name cannot be empty.",
          nameof(movie)
          );
    }

    bool alreadyExists = _movies.Any(existingMovie =>
        existingMovie.Name.Equals(
          normalizedName,
          StringComparison.OrdinalIgnoreCase
          )
        && existingMovie.AddedBy.DiscordId == movie.AddedBy.DiscordId
        );

    if (alreadyExists)
    {
      return false;
    }

    Movie normalizedMovie = new()
    {
      Name = normalizedName,
      AddedBy = movie.AddedBy
    };

    _movies.Add(normalizedMovie);
    return true;
  }

  public IReadOnlyCollection<Movie> GetAllMovies()
  {
    return _movies.AsReadOnly();
  }

  public IReadOnlyCollection<Movie> GetMoviesByPerson(ulong discordId)
  {
    return _movies
      .Where(movie => movie.AddedBy.DiscordId == discordId)
      .ToList()
      .AsReadOnly();
  }

  public Movie? GetRandomMovie()
  {
    if (_movies.Count == 0)
    {
      return null;
    }

    int randomIndex = Random.Shared.Next(_movies.Count);

    return _movies[randomIndex];
  }

  public Movie? GetRandomMovieByPerson(ulong discordId)
  {
    List<Movie> matchingMovies = _movies
      .Where(movie => movie.AddedBy.DiscordId == discordId)
      .ToList();

    if (matchingMovies.Count == 0)
    {
      return null;
    }

    int randomIndex = Random.Shared.Next(matchingMovies.Count);

    return matchingMovies[randomIndex];
  }
}
