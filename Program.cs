using DotNetEnv;

public class Program
{
  public static async Task Main()
  {
    Env.Load();
    var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
    if (string.IsNullOrEmpty(token))
    {
      throw new Exception("Discord token is missing");
    }

    var bot = new Bot(token);

    await bot.StartAsync();
  }
}
