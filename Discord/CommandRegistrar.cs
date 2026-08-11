using Discord;
using Discord.Rest;

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
            if (clearExistingCommands)
            {
                await ClearCommandsAsync(serverId);
            }

            await RegisterCommandsAsync(serverId);
        }
    }

    private async Task ClearCommandsAsync(ulong serverId)
    {
        var commands =
            await _restClient.GetGuildApplicationCommands(
                serverId);

        foreach (var command in commands)
        {
            await command.DeleteAsync();

            Console.WriteLine(
                $"Deleted /{command.Name} from guild {serverId}");
        }
    }

    private async Task RegisterCommandsAsync(
        ulong serverId)
    {
        foreach (SlashCommandBuilder command in BuildCommands())
        {
            await _restClient.CreateGuildCommand(
                command.Build(),
                serverId);
        }
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
                "nickname",
                ApplicationCommandOptionType.String,
                "The nickname to use",
                isRequired: true);
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
                        "name",
                        ApplicationCommandOptionType.String,
                        "The movie title",
                        isRequired: true)
                    .AddOption(
                        "year",
                        ApplicationCommandOptionType.Integer,
                        "The movie's release year",
                        isRequired: false))

            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("list")
                    .WithDescription("Shows all movies")
                    .WithType(ApplicationCommandOptionType.SubCommand))

            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("pick")
                    .WithDescription("Picks a random movie")
                    .WithType(ApplicationCommandOptionType.SubCommand))

            .AddOption(
                new SlashCommandOptionBuilder()
                    .WithName("watched")
                    .WithDescription("Marks a movie as watched")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(
                        "name",
                        ApplicationCommandOptionType.String,
                        "The movie title",
                        isRequired: true));
    }
}