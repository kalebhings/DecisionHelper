using Discord;
using Discord.WebSocket;

public class Bot
{
  private readonly DiscordSocketClient _client;
  private readonly string _token;
  private readonly CommandHandler _commandHandler;

  public Bot(string token)
  {
    _token = token;
    _client = new DiscordSocketClient();
    _commandHandler = new CommandHandler();
  }
  
  public async Task StartAsync()
  {
    _client.Log += LogAsync;

    await _client.LoginAsync(
          TokenType.Bot,
          _token
          );
    await _client.StartAsync();
    _client.InteractionCreated += _commandHandler.HandleCommand;

    await Task.Delay(-1);
  }

  private Task LogAsync(LogMessage message)
  {
    Console.WriteLine(message);
    return Task.CompletedTask;
  }

}
