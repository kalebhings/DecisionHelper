using Discord.WebSocket;
using DecisionHelper.Services;

public class ListMoviesCommand : ICommand
{
    private readonly MovieService _movieService;

    public string Name => "listmovies";

    public ListMoviesCommand(MovieService movieService)
    {
        _movieService = movieService;
    }

    public async Task ExecuteAsync(SocketSlashCommand command)
    {
        var movies = await _movieService.GetAllMoviesAsync();

        if (movies.Count == 0)
        {
            await command.RespondAsync(
                "There aren't any movies on the list yet.",
                ephemeral: true);

            return;
        }

        var lines = movies.Select(movie =>
        {
            string year = movie.ReleaseYear.HasValue
                ? $" ({movie.ReleaseYear})"
                : string.Empty;

            string addedBy = movie.AddedBy?.Nickname ?? "Unknown";

            return $"• **{movie.Title}**{year} — added by {addedBy}";
        });

        string response =
            $"🎬 **Movie List ({movies.Count})**\n\n" +
            string.Join("\n", lines);

        await command.RespondAsync(response);
    }
}
