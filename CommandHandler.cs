using Discord.WebSocket;
using DecisionHelper.Services;

public class CommandHandler
{
  private readonly Dictionary<string, ICommand> _commands;

  public CommandHandler(
      MovieService movieService,
      PersonService personService
      )
  {
    var commands = new List<ICommand>
    {
      new PingCommand(),
      new SetNicknameCommand(personService),
      new MovieCommand(
          movieService,
          personService)
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

    if (_commands.TryGetValue(
          command.CommandName,
          out var commandHandler
          ))
    {
      await commandHandler.ExecuteAsync(command);
    }
    
  }
}
