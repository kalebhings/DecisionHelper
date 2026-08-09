using Discord.WebSocket;
using DecisionHelper.Services;

public class PickMovieCommand : ICommand
{
  private readonly MovieService _movieService;

  public string Name => "pickmovie";

  public PickMovieCommand(MovieService movieService)
  {
    _movieService = movieService;
  }

  public async Task ExecuteAsync(SocketSlashCommand command)
  {
    var movie = await _movieService.GetRandomMovieAsync();

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
        $"**Tonights movie:**\n\n" +
        $"**{movie.Title}**{year}\n" +
        $"AddedBy {addedBy}");
  }
}
