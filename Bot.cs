using Discord;
using Discord.Rest;
using Discord.WebSocket;

public class Bot
{
  private readonly DiscordSocketClient _client;
  private readonly DiscordRestClient _restClient;
  private readonly string _token;
  private readonly CommandHandler _commandHandler;
  private readonly string _serverId;

  public Bot(string token, string serverId)
  {
    _token = token;
    _client = new DiscordSocketClient();
    _restClient = new DiscordRestClient();
    _commandHandler = new CommandHandler();
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
    var command = new SlashCommandBuilder()
      .WithName("ping")
      .WithDescription("Replies with pong!");

    await _restClient.CreateGuildCommand(
        command.Build(),
        ulong.Parse(_serverId)
        );
  }

}
