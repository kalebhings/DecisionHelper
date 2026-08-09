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

      Movie? movie =
          await _movieService.AddMovieAsync(
              movieName,
              releaseYear: null,
              addedByPersonId: person.Id);

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
