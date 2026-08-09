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
    var pingCommand = new SlashCommandBuilder()
      .WithName("ping")
      .WithDescription("Replies with pong!");

    var setNicknameCommand = new SlashCommandBuilder()
      .WithName("setnickname")
      .WithDescription("Sets your nickname")
      .AddOption(
          "nickname",
          ApplicationCommandOptionType.String,
          "The nickname to use",
          isRequired: true
          );

    var addMovieCommand = new SlashCommandBuilder()
      .WithName("addmovie")
      .WithDescription("Adds a movie to your list")
      .AddOption(
          "name",
          ApplicationCommandOptionType.String,
          "The name of the movie",
          isRequired: true
          )
      .AddOption(
          "year",
          ApplicationCommandOptionType.Integer,
          "The movie's release year",
          isRequired: false
          );

    var listMoviesCommand = new SlashCommandBuilder()
      .WithName("listmovies")
      .WithDescription("Shows all movies on the movie list.");

    var pickMovieCommand = new SlashCommandBuilder()
      .WithName("pickmovie")
      .WithDescription("Picks a random movie from the movie list.");

    ulong serverId = ulong.Parse(_serverId);

    await _restClient.CreateGuildCommand(
        pingCommand.Build(),
        serverId
        );

    await _restClient.CreateGuildCommand(
        setNicknameCommand.Build(),
        serverId
        );

    await _restClient.CreateGuildCommand(
          addMovieCommand.Build(),
          serverId
          );

    await _restClient.CreateGuildCommand(
        listMoviesCommand.Build(),
        serverId
        );

    await _restClient.CreateGuildCommand(
        pickMovieCommand.Build(),
        serverId
        );
        
  }

}
