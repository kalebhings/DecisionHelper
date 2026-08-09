using DecisionHelper.Data;
using DecisionHelper.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main()
    {
        Env.Load();

        string? token =
            Environment.GetEnvironmentVariable(
                "DISCORD_TOKEN");

        string? serverId =
            Environment.GetEnvironmentVariable(
                "DISCORD_SERVER_ID");

        string connectionString =
            Environment.GetEnvironmentVariable(
                "DATABASE_CONNECTION_STRING")
            ?? "Data Source=decision-helper.db";

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Discord token is missing.");
        }

        if (string.IsNullOrWhiteSpace(serverId))
        {
            throw new InvalidOperationException(
                "Discord server ID is missing.");
        }

        var services = new ServiceCollection();

        services.AddDbContextFactory<DecisionHelperDbContext>(
            options =>
                options.UseSqlite(connectionString));

        await using var serviceProvider =
            services.BuildServiceProvider();

        var dbContextFactory =
            serviceProvider.GetRequiredService<
                IDbContextFactory<DecisionHelperDbContext>>();

        var personService =
            new PersonService(dbContextFactory);

        var movieService =
            new MovieService(dbContextFactory);

        var commandHandler =
            new CommandHandler(
                movieService,
                personService);

        var bot = new Bot(
            token,
            serverId,
            commandHandler);

        await bot.StartAsync();
    }
}