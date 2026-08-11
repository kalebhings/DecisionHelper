using Discord;
using Discord.Rest;
using DecisionHelper.Services;

namespace DecisionHelper.Discord;

public class CommandRegistrar
{
    private readonly DiscordRestClient _restClient;
    private readonly IReadOnlyCollection<ulong> _serverIds;

    public CommandRegistrar(
        DiscordRestClient restClient,
        IReadOnlyCollection<ulong> serverIds)
    {
        _restClient = restClient;
        _serverIds = serverIds;
    }


    public async Task RegisterAsync(
        bool clearExistingCommands = false)
    {
        foreach (ulong serverId in _serverIds)
        {
            await RegisterCommandsAsync(serverId, clearExistingCommands);
        }
    }

    private async Task RegisterCommandsAsync(
        ulong serverId,
        bool clearExistingCommands)
    {
        RestGuild guild = await _restClient.GetGuildAsync(serverId);
        ApplicationCommandProperties[] commands = BuildCommands()
            .Select(command => command.Build())
            .ToArray();

        await guild.BulkOverwriteApplicationCommandsAsync(commands);

        string action = clearExistingCommands
            ? "Cleared stale commands and registered"
            : "Registered";

        Console.WriteLine(
            $"{action} {commands.Length} commands in guild {serverId}");
    }

    private static SlashCommandBuilder[] BuildCommands()
    {
        return
        [
            BuildPingCommand(),
            BuildSetNicknameCommand(),
            BuildMovieCommand()
        ];
    }

    private static SlashCommandBuilder BuildPingCommand()
    {
        return new SlashCommandBuilder()
            .WithName("ping")
            .WithDescription("Replies with pong!");
    }

    private static SlashCommandBuilder BuildSetNicknameCommand()
    {
        return new SlashCommandBuilder()
            .WithName("setnickname")
            .WithDescription("Sets your nickname")
            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("nickname")
                    .WithDescription("The nickname to use")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true)
                    .WithMaxLength(InputValidator.MaxNicknameLength));
    }

    private static SlashCommandBuilder BuildMovieCommand()
    {
        return new SlashCommandBuilder()
            .WithName("movie")
            .WithDescription("Manage and choose movies")

            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("add")
                    .WithDescription("Adds a movie to the list")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("name")
                            .WithDescription("The movie title")
                            .WithType(ApplicationCommandOptionType.String)
                            .WithRequired(true)
                            .WithMaxLength(InputValidator.MaxMovieTitleLength))
                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("year")
                            .WithDescription("The movie's release year")
                            .WithType(ApplicationCommandOptionType.Integer)
                            .WithRequired(false)
                            .WithMinValue(1900)
                            .WithMaxValue(2100)))

            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("list")
                    .WithDescription("Shows movies")
                    .WithType(ApplicationCommandOptionType.SubCommand)

                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("added-by")
                            .WithDescription(
                                "Only show movies added by this bot nickname")
                            .WithType(ApplicationCommandOptionType.String)
                            .WithRequired(false)
                            .WithMaxLength(InputValidator.MaxNicknameLength))

                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("status")
                            .WithDescription("Filter by your watch status")
                            .WithType(ApplicationCommandOptionType.String)
                            .WithRequired(false)
                            .AddChoice("Watched", "watched")
                            .AddChoice("Unwatched", "unwatched")))

            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("pick")
                    .WithDescription("Picks a random movie")
                    .WithType(ApplicationCommandOptionType.SubCommand)

                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("added-by")
                            .WithDescription(
                                "Only pick movies added by this bot nickname")
                            .WithType(ApplicationCommandOptionType.String)
                            .WithRequired(false)
                            .WithMaxLength(InputValidator.MaxNicknameLength))

                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("status")
                            .WithDescription("Filter by your watch status")
                            .WithType(ApplicationCommandOptionType.String)
                            .WithRequired(false)
                            .AddChoice("Watched", "watched")
                            .AddChoice("Unwatched", "unwatched")))

            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("watched")
                    .WithDescription("Marks a movie as watched")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("name")
                            .WithDescription("The movie title")
                            .WithType(ApplicationCommandOptionType.String)
                            .WithRequired(true)
                            .WithMaxLength(InputValidator.MaxMovieTitleLength))
                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("year")
                            .WithDescription("Required when releases share a title")
                            .WithType(ApplicationCommandOptionType.Integer)
                            .WithRequired(false)
                            .WithMinValue(1900)
                            .WithMaxValue(2100)));
    }
}
