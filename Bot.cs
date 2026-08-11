using DecisionHelper.Discord;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

public sealed class Bot : IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordRestClient _restClient;
    private readonly string _token;
    private readonly CommandHandler _commandHandler;
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
        _commandRegistrar = new CommandRegistrar(_restClient, serverIds);
    }

    public async Task StartAsync(
        bool clearCommands = false,
        CancellationToken cancellationToken = default)
    {
        _client.Log += LogAsync;
        _client.InteractionCreated += _commandHandler.HandleCommand;

        try
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _restClient.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
            await _commandRegistrar.RegisterAsync(clearCommands);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            await _client.StopAsync();
            await _client.LogoutAsync();
            await _restClient.LogoutAsync();
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _restClient.Dispose();
    }

    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}
