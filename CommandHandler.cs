using Discord.WebSocket;

public class CommandHandler
{
  public async Task HandleCommand(SocketInteraction interaction)
  {
    if (interaction is SocketSlashCommand command)
    {
      if (command.CommandName == "ping")
      {
        await command.RespondAsync("Pong!");
      }
    }
  }
}
