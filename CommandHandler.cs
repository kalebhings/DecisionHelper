using DecisionHelper.Discord;
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
            try
            {
                await commandHandler.ExecuteAsync(command);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    $"Command /{command.CommandName} failed for user " +
                    $"{command.User.Id} in guild {command.GuildId}: {exception}");

                const string message =
                    "Something went wrong while processing that command. Please try again.";

                try
                {
                    if (command.HasResponded)
                    {
                        await InteractionResponses.CompleteAsync(command, message);
                    }
                    else
                    {
                        await InteractionResponses.RespondAsync(
                            command,
                            message,
                            ephemeral: true);
                    }
                }
                catch (Exception responseException)
                {
                    Console.Error.WriteLine(
                        $"Failed to send command error response: {responseException}");
                }
            }
        }

    }
}
