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

  public async Task ExecuteAsync(SocketSlashCommand command)
  {
    var nameOption = command.Data.Options
      .FirstOrDefault(option => option.Name == "name");

    string? movieName = nameOption?.Value?.ToString();

    if (string.IsNullOrWhiteSpace(movieName))
    {
      await command.RespondAsync(
          "Please provide a movie name",
          ephemeral: true
          );

      return;
    }

    string nickname = 
      command.User.GlobalName
      ?? command.User.Username;

    string defaultNickname =
      command.User.GlobalName
      ?? command.User.Username;

    Person person = _personService.GetOrCreatePerson(
        command.User.Id,
        defaultNickname
        );

    var movie = new Movie
    {
      Name = movieName,
      AddedBy = person
    };

    bool wasAdded = _movieService.AddMovie(movie);

    if (!wasAdded)
    {
      await command.RespondAsync(
            $"**{movieName.Trim()} is already on your list.",
            ephemeral: true
            );

      return;
    }

    await command.RespondAsync(
        $"Added **{movieName.Trim()}** to {nickname}'s movie list."
        );
  }
}
