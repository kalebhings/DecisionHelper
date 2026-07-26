using DotNetEnv;

public class Program
{
  public static async Task Main()
  {
    Env.Load();
    var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
    var serverId = Environment.GetEnvironmentVariable("DISCORD_SERVER_ID");
    if (string.IsNullOrEmpty(token))
    {
      throw new Exception("Discord token is missing");
    }
    if (string.IsNullOrEmpty(serverId))
    {
      throw new Exception("Discord server ID is missing");
    }

    var bot = new Bot(token, serverId);

    await bot.StartAsync();
  }
}
