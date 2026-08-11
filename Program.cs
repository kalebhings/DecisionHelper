using DecisionHelper.Configuration;
using DecisionHelper.Data;
using DecisionHelper.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main(string[] args)
    {
        Env.NoClobber().Load();
        AppConfiguration configuration = AppConfiguration.FromEnvironment();

        var services = new ServiceCollection();

        services.AddDbContextFactory<DecisionHelperDbContext>(options =>
            options.UseSqlite(configuration.DatabaseConnectionString));

        await using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        IDbContextFactory<DecisionHelperDbContext> dbContextFactory =
            serviceProvider.GetRequiredService<
                IDbContextFactory<DecisionHelperDbContext>>();

        await using (DecisionHelperDbContext db =
            await dbContextFactory.CreateDbContextAsync())
        {
            await db.Database.MigrateAsync();
        }

        await new LegacyDataMigrator(dbContextFactory)
            .MigrateAsync(configuration.ServerIds);

        if (args.Contains(
            "--migrate-only",
            StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var personService = new PersonService(dbContextFactory);
        var movieService = new MovieService(dbContextFactory);
        var commandHandler = new CommandHandler(movieService, personService);

        bool clearCommands = args.Contains(
            "--clear-commands",
            StringComparer.OrdinalIgnoreCase);

        using var cancellationSource = new CancellationTokenSource();

        ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationSource.Cancel();
        };

        Console.CancelKeyPress += cancelHandler;

        using var bot = new Bot(
            configuration.DiscordToken,
            configuration.ServerIds,
            commandHandler);

        try
        {
            await bot.StartAsync(
                clearCommands,
                cancellationSource.Token);
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
        }
    }
}
