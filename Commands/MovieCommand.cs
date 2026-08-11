using DecisionHelper.Discord;
using DecisionHelper.Models;
using DecisionHelper.Services;
using Discord;
using Discord.WebSocket;

public class MovieCommand : ICommand
{
    private const int MaxPageLength = 1900;

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
        if (!command.GuildId.HasValue)
        {
            await InteractionResponses.RespondAsync(
                command,
                "Movie commands can only be used in a server.",
                ephemeral: true);
            return;
        }

        SocketSlashCommandDataOption? subcommand =
            command.Data.Options.FirstOrDefault();

        if (subcommand is null)
        {
            await InteractionResponses.RespondAsync(
                command,
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
                await ListMoviesAsync(command, subcommand);
                break;
            case "pick":
                await PickMovieAsync(command, subcommand);
                break;
            case "watched":
                await MarkWatchedAsync(command, subcommand);
                break;
            default:
                await InteractionResponses.RespondAsync(
                    command,
                    "Unknown movie command.",
                    ephemeral: true);
                break;
        }
    }

    private async Task AddMovieAsync(
        SocketSlashCommand command,
        SocketSlashCommandDataOption subcommand)
    {
        string? movieName = GetStringOption(subcommand, "name");
        long? yearValue = GetLongOption(subcommand, "year");

        if (!TryValidateMovieInput(movieName, yearValue, out string title))
        {
            await InteractionResponses.RespondAsync(
                command,
                "Provide a movie title of 200 characters or fewer and a release year between 1900 and 2100.",
                ephemeral: true);
            return;
        }

        int? releaseYear = yearValue.HasValue ? (int)yearValue.Value : null;

        await command.DeferAsync();

        Person person = await GetCurrentPersonAsync(command);
        Movie? movie = await _movieService.AddMovieAsync(
            command.GuildId!.Value,
            title,
            releaseYear,
            person.Id);

        if (movie is null)
        {
            await InteractionResponses.CompleteAsync(
                command,
                $"**{Safe(title)}**{FormatYear(releaseYear)} is already on the movie list.");
            return;
        }

        await InteractionResponses.CompleteAsync(
            command,
            $"Added **{Safe(movie.Title, InputValidator.MaxMovieTitleLength)}**{FormatYear(movie.ReleaseYear)} " +
            $"to the movie list. Added by: **{Safe(person.Nickname, InputValidator.MaxNicknameLength)}**.");
    }

    private async Task ListMoviesAsync(
        SocketSlashCommand command,
        SocketSlashCommandDataOption subcommand)
    {
        await command.DeferAsync();

        MovieFilter? filter = await BuildMovieFilterAsync(command, subcommand);

        if (filter is null)
        {
            await InteractionResponses.CompleteAsync(
                command,
                "There aren't any movies matching those filters.");
            return;
        }

        IReadOnlyList<Movie> movies = await _movieService.GetMoviesAsync(
            command.GuildId!.Value,
            filter);

        if (movies.Count == 0)
        {
            await InteractionResponses.CompleteAsync(
                command,
                "There aren't any movies matching those filters.");
            return;
        }

        IReadOnlyList<string> pages = BuildMoviePages(movies);
        await InteractionResponses.CompleteAsync(command, pages[0]);

        foreach (string page in pages.Skip(1))
        {
            await InteractionResponses.FollowupAsync(command, page);
        }
    }

    private async Task PickMovieAsync(
        SocketSlashCommand command,
        SocketSlashCommandDataOption subcommand)
    {
        await command.DeferAsync();

        MovieFilter? filter = await BuildMovieFilterAsync(command, subcommand);

        if (filter is null)
        {
            await InteractionResponses.CompleteAsync(
                command,
                "There aren't any movies matching those filters.");
            return;
        }

        Movie? movie = await _movieService.GetRandomMovieAsync(
            command.GuildId!.Value,
            filter);

        if (movie is null)
        {
            await InteractionResponses.CompleteAsync(
                command,
                "There aren't any movies matching those filters.");
            return;
        }

        string addedBy = movie.AddedBy?.Nickname ?? "Unknown";

        await InteractionResponses.CompleteAsync(
            command,
            "**Tonight's movie:**\n\n" +
            $"**{Safe(movie.Title, InputValidator.MaxMovieTitleLength)}**{FormatYear(movie.ReleaseYear)}\n" +
            $"Added by: {Safe(addedBy, InputValidator.MaxNicknameLength)}");
    }

    private async Task MarkWatchedAsync(
        SocketSlashCommand command,
        SocketSlashCommandDataOption subcommand)
    {
        string? movieName = GetStringOption(subcommand, "name");
        long? yearValue = GetLongOption(subcommand, "year");

        if (!TryValidateMovieInput(movieName, yearValue, out string title))
        {
            await InteractionResponses.RespondAsync(
                command,
                "Provide a movie title of 200 characters or fewer and a release year between 1900 and 2100.",
                ephemeral: true);
            return;
        }

        int? releaseYear = yearValue.HasValue ? (int)yearValue.Value : null;

        await command.DeferAsync();

        Person person = await GetCurrentPersonAsync(command);
        MovieWatchResult result = await _movieService.MarkMovieWatchedAsync(
            command.GuildId!.Value,
            title,
            releaseYear,
            person.Id);

        string safeTitle = $"**{Safe(title, InputValidator.MaxMovieTitleLength)}**{FormatYear(releaseYear)}";
        string response = result switch
        {
            MovieWatchResult.MovieNotFound =>
                $"I couldn't find {safeTitle}.",
            MovieWatchResult.AmbiguousMovie =>
                $"More than one movie is named **{Safe(title, InputValidator.MaxMovieTitleLength)}**. Include its release year.",
            MovieWatchResult.AlreadyWatched =>
                $"You've already marked {safeTitle} as watched.",
            _ =>
                $"{safeTitle} marked as watched by **{Safe(person.Nickname, InputValidator.MaxNicknameLength)}**."
        };

        await InteractionResponses.CompleteAsync(command, response);
    }

    private async Task<MovieFilter?> BuildMovieFilterAsync(
        SocketSlashCommand command,
        SocketSlashCommandDataOption subcommand)
    {
        string? addedByNickname = GetStringOption(subcommand, "added-by");
        IReadOnlyCollection<int>? addedByPersonIds = null;

        if (!string.IsNullOrWhiteSpace(addedByNickname))
        {
            addedByPersonIds = await _personService.GetPersonIdsByNicknameAsync(
                command.GuildId!.Value,
                addedByNickname);

            if (addedByPersonIds.Count == 0)
            {
                return null;
            }
        }

        string? statusValue = GetStringOption(subcommand, "status");
        WatchFilter watchFilter = statusValue switch
        {
            null => WatchFilter.Any,
            "watched" => WatchFilter.Watched,
            "unwatched" => WatchFilter.Unwatched,
            _ => throw new ArgumentException("Unknown watch status.")
        };

        int? watchStatusPersonId = null;

        if (watchFilter != WatchFilter.Any)
        {
            watchStatusPersonId = (await GetCurrentPersonAsync(command)).Id;
        }

        return new MovieFilter
        {
            AddedByPersonIds = addedByPersonIds,
            WatchStatusPersonId = watchStatusPersonId,
            WatchStatus = watchFilter
        };
    }

    private async Task<Person> GetCurrentPersonAsync(
        SocketSlashCommand command)
    {
        string defaultNickname = command.User.GlobalName
            ?? command.User.Username;

        return await _personService.GetOrCreatePersonAsync(
            command.GuildId!.Value,
            command.User.Id,
            defaultNickname);
    }

    private static IReadOnlyList<string> BuildMoviePages(
        IReadOnlyList<Movie> movies)
    {
        var lines = new List<string>
        {
            $"**Movie List ({movies.Count})**"
        };

        foreach (IGrouping<string, Movie> group in movies
            .GroupBy(movie => movie.AddedBy?.Nickname ?? "Unknown")
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
        {
            lines.Add(string.Empty);
            lines.Add($"**{Safe(group.Key, InputValidator.MaxNicknameLength)}**");

            lines.AddRange(group
                .OrderBy(movie => movie.Title, StringComparer.OrdinalIgnoreCase)
                .Select(movie =>
                    $"• **{Safe(movie.Title, InputValidator.MaxMovieTitleLength)}**{FormatYear(movie.ReleaseYear)}"));
        }

        var pages = new List<string>();
        var currentPage = new System.Text.StringBuilder();

        foreach (string line in lines)
        {
            int additionalLength = line.Length +
                (currentPage.Length == 0 ? 0 : Environment.NewLine.Length);

            if (currentPage.Length > 0 &&
                currentPage.Length + additionalLength > MaxPageLength)
            {
                pages.Add(currentPage.ToString());
                currentPage.Clear();
            }

            if (currentPage.Length > 0)
            {
                currentPage.AppendLine();
            }

            currentPage.Append(line);
        }

        pages.Add(currentPage.ToString());
        return pages;
    }

    private static bool TryValidateMovieInput(
        string? movieName,
        long? year,
        out string title)
    {
        title = movieName?.Trim() ?? string.Empty;

        return title.Length is > 0 and <= InputValidator.MaxMovieTitleLength &&
            !title.Any(char.IsControl) &&
            (!year.HasValue || year.Value is >= 1900 and <= 2100);
    }

    private static string? GetStringOption(
        SocketSlashCommandDataOption subcommand,
        string name)
    {
        return subcommand.Options
            .FirstOrDefault(option => option.Name == name)
            ?.Value?.ToString();
    }

    private static long? GetLongOption(
        SocketSlashCommandDataOption subcommand,
        string name)
    {
        return subcommand.Options
            .FirstOrDefault(option => option.Name == name)
            ?.Value as long?;
    }

    private static string Safe(string value) => Format.Sanitize(value);

    private static string Safe(string value, int maxLength)
    {
        string bounded = value.Length <= maxLength
            ? value
            : string.Concat(value.AsSpan(0, maxLength - 3), "...");

        return Safe(bounded);
    }

    private static string FormatYear(int? releaseYear) =>
        releaseYear.HasValue ? $" ({releaseYear})" : string.Empty;
}
