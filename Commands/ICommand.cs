using Discord.WebSocket;

public interface ICommand
{
    string Name { get; }

    Task ExecuteAsync(SocketSlashCommand command);
}
