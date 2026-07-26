using Discord.WebSocket;

public class CommandHandler
{
  private readonly Dictionary<string, ICommand> _commands;

  public CommandHandler()
  {
    var commands = new List<ICommand>
    {
      new PingCommand()
    };

    _commands = commands.ToDictionary(
        command => command.Name
    );
  }

  public async Task HandleCommand(SocketInteraction interaction)
  {
    if (interaction is not SocketSlashCommand command)
    {
      return;
    }
    
    if (_commands.TryGetValue(command.CommandName, out var commandHandler))
    {
      await commandHandler.ExecuteAsync(command);
    }
  }
}
