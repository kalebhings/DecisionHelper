using Discord.WebSocket;

public class PingCommand : ICommand
{
  public string Name => "ping";

  public async Task ExecuteAsync(SocketSlashCommand command)
  {
    await command.RespondAsync("Pong!");
  }
}
