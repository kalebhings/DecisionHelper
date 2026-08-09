using Discord.WebSocket;
using DecisionHelper.Models;
using DecisionHelper.Services;

public class AddMovieCommand : ICommand
{
  private readonly MovieService _movieService;
  private readonly PersonService _personService;

  public string Name => "addmovie";

  public AddMovieCommand(
      MovieService movieService,
      PersonService personService
      )
  {
    _movieService = movieService;
    _personService = personService;
  }

  public async Task ExecuteAsync(
      SocketSlashCommand command)
  {
      var nameOption = command.Data.Options
          .FirstOrDefault(option =>
              option.Name == "name");

      var yearOption = command.Data.Options
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
      if (yearOption?.Value is long yearValue)
      {
        releaseYear = checked((int)yearValue);
      }

      if (releaseYear is < 1900 or > 2100)
      {
        await command.RespondAsync(
            "Please provide a valid movie release year. Must be between 1900 or 2100",
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

      await command.RespondAsync(
          $"Added **{movie.Title}** to the movie list. " +
          $"Added by **{person.Nickname}**.");
  }
}
