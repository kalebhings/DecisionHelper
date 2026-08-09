using Discord.WebSocket;
using DecisionHelper.Models;
using DecisionHelper.Services;

public class MovieCommand : ICommand
{
  private readonly MovieService _movieService;
  private readonly PersonService _personService;

  public string Name => "movie";

    public MovieCommand(
        MovieService movieService,
        PersonService personService)
    {
        _movieService = movieService;
        _personService = personService;
    }

  public async Task ExecuteAsync(SocketSlashCommand command)
  {
    var subcommand = command.Data.Options.FirstOrDefault();

    if (subcommand is null)
    {
      await command.RespondAsync(
          "Please choose a movie command.",
          ephemeral: true);
      return;
    }

    switch (subcommand.Name)
    {
      case "add":
        await AddMovieAsync(command, subcommand);
        break;

      case "list":
        await ListMoviesAsync(command);
        break;

      case "pick":
        await PickMovieAsync(command);
        break;

      case "watched":
        await MarkWatchedAsync(command, subcommand);
        break;

      default:
        await command.RespondAsync(
            "Unknown movie command.",
            ephemeral: true);

        break;
    }
  }

  private async Task AddMovieAsync(
      SocketSlashCommand command,
      SocketSlashCommandDataOption subcommand
      )
  {
    var nameOption = subcommand.Options
      .FirstOrDefault(option =>
          option.Name == "name");

    var yearOption = subcommand.Options
      .FirstOrDefault(option =>
          option.Name == "year");

    string? movieName =
      nameOption?.Value?.ToString();

    if (string.IsNullOrWhiteSpace(movieName))
    {
      await command.RespondAsync(
          "Please provide a movie name.",
          ephemeral: true);
      
      return;
    }

    int? releaseYear = null;
    if (yearOption?.Value is long year)
    {
      releaseYear = checked((int)year);
    }

    if (releaseYear is < 1900 or > 2100)
    {
      await command.RespondAsync(
          "please provide a valid release year between 1900 and 2100.",
          ephemeral: true);
      return;
    }

    string defaultNickname =
      command.User.GlobalName
      ?? command.User.Username;

    Person person = 
      await _personService.GetOrCreatePersonAsync(
          command.User.Id,
          defaultNickname);
    
    Movie? movie =
      await _movieService.AddMovieAsync(
          movieName,
          releaseYear,
          person.Id);
    
    if (movie is null)
    {
      await command.RespondAsync(
          $"**{movieName.Trim()}** is already on the movie list.",
          ephemeral: true);
      return;
    }

    string yearText = movie.ReleaseYear.HasValue
      ? $" ({movie.ReleaseYear})"
      : string.Empty;

    await command.RespondAsync(
        $"Added **{movie.Title}**{yearText} " +
        $"to the movie list. Added by: **{person.Nickname}**.");
  }

    private async Task ListMoviesAsync(
        SocketSlashCommand command)
    {
        var movies =
            await _movieService.GetAllMoviesAsync();

        if (movies.Count == 0)
        {
            await command.RespondAsync(
                "There aren't any movies on the list yet.",
                ephemeral: true);

            return;
        }

        var groupedMovies = movies
            .GroupBy(movie =>
                movie.AddedBy?.Nickname ?? "Unknown")
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        var sections = groupedMovies.Select(group =>
        {
            var movieLines = group
                .OrderBy(movie => movie.Title, StringComparer.OrdinalIgnoreCase)
                .Select(movie =>
                {
                    string year = movie.ReleaseYear.HasValue
                        ? $" ({movie.ReleaseYear})"
                        : string.Empty;

                    return $"• **{movie.Title}**{year}";
                });

            return $"**{group.Key}**\n" +
                string.Join("\n", movieLines);
        });

        await command.RespondAsync(
            $"**Movie List ({movies.Count})**\n\n" +
            string.Join("\n\n", sections));
    }

    private async Task PickMovieAsync(
        SocketSlashCommand command)
    {
        Movie? movie =
            await _movieService.GetRandomMovieAsync();

        if (movie is null)
        {
            await command.RespondAsync(
                "There aren't any movies to pick from yet.",
                ephemeral: true);

            return;
        }

        string year = movie.ReleaseYear.HasValue
            ? $" ({movie.ReleaseYear})"
            : string.Empty;

        string addedBy =
            movie.AddedBy?.Nickname ?? "Unknown";

        await command.RespondAsync(
            $"**Tonight's movie:**\n\n" +
            $"**{movie.Title}**{year}\n" +
            $"Added by: {addedBy}");
    }

    private async Task MarkWatchedAsync(
        SocketSlashCommand command,
        SocketSlashCommandDataOption subcommand)
    {
        var nameOption = subcommand.Options
            .FirstOrDefault(option =>
                option.Name == "name");

        string? movieName =
            nameOption?.Value?.ToString();

        if (string.IsNullOrWhiteSpace(movieName))
        {
            await command.RespondAsync(
                "Please provide a movie name.",
                ephemeral: true);

            return;
        }

        string defaultNickname =
            command.User.GlobalName
            ?? command.User.Username;

        Person person =
            await _personService.GetOrCreatePersonAsync(
                command.User.Id,
                defaultNickname);

        MovieWatchResult result =
            await _movieService.MarkMovieWatchedAsync(
                movieName,
                person.Id);

        switch (result)
        {
            case MovieWatchResult.MovieNotFound:
                await command.RespondAsync(
                    $"I couldn't find **{movieName.Trim()}**.",
                    ephemeral: true);
                break;

            case MovieWatchResult.AlreadyWatched:
                await command.RespondAsync(
                    $"You've already marked **{movieName.Trim()}** as watched.",
                    ephemeral: true);
                break;

            case MovieWatchResult.MarkedWatched:
                await command.RespondAsync(
                    $"**{movieName.Trim()}** marked as watched by " +
                    $"**{person.Nickname}**.");
                break;
        }
    }
}
