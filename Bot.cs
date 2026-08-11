using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DecisionHelper.Services;
using DecisionHelper.Discord;

public class Bot
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordRestClient _restClient;
    private readonly string _token;
    private readonly CommandHandler _commandHandler;
    private readonly IReadOnlyCollection<ulong> _serverIds;

    private readonly CommandRegistrar _commandRegistrar;

    public Bot(
        string token,
        IReadOnlyCollection<ulong> serverIds,
        CommandHandler commandHandler)
    {
        _token = token;

        _client = new DiscordSocketClient();

        _restClient = new DiscordRestClient();

        _commandHandler = commandHandler;

        _serverIds = serverIds;

        _commandRegistrar = new CommandRegistrar(
        _restClient,
        serverIds);
    }
  
    public async Task StartAsync(
        bool clearCommands = false)
    {
        _client.Log += LogAsync;

        _client.InteractionCreated +=
            _commandHandler.HandleCommand;

        await _client.LoginAsync(
            TokenType.Bot,
            _token);

        await _restClient.LoginAsync(
            TokenType.Bot,
            _token);

        await _client.StartAsync();

        await _commandRegistrar.RegisterAsync(
            clearCommands);

        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage message)
    {
    Console.WriteLine(message);
    return Task.CompletedTask;
    }

}
