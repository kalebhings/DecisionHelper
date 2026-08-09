using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DecisionHelper.Services;

public class Bot
{
  private readonly DiscordSocketClient _client;
  private readonly DiscordRestClient _restClient;
  private readonly string _token;
  private readonly CommandHandler _commandHandler;
  private readonly string _serverId;

  public Bot(
      string token,
      string serverId,
      CommandHandler commandHandler)
  {
      _token = token;

      _client = new DiscordSocketClient();

      _restClient = new DiscordRestClient();

      _commandHandler = commandHandler;

      _serverId = serverId;
  }
  
  public async Task StartAsync()
  {
    _client.Log += LogAsync;

    _client.InteractionCreated += _commandHandler.HandleCommand;

    await _client.LoginAsync(
          TokenType.Bot,
          _token
          );
    await _restClient.LoginAsync(
        TokenType.Bot,
        _token
        );
    await _client.StartAsync();

    await RegisterCommandsAsync();

    await Task.Delay(-1);
  }

  private Task LogAsync(LogMessage message)
  {
    Console.WriteLine(message);
    return Task.CompletedTask;
  }


  private async Task RegisterCommandsAsync()
  {
      ulong serverId = ulong.Parse(_serverId);

      SlashCommandBuilder[] commands = BuildCommands();

      foreach (SlashCommandBuilder command in commands)
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
