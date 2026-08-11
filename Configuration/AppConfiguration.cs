namespace DecisionHelper.Configuration;

public sealed record AppConfiguration(
    string DiscordToken,
    IReadOnlyList<ulong> ServerIds,
    string DatabaseConnectionString)
{
    public static AppConfiguration FromEnvironment()
    {
        string token = GetRequired("DISCORD_TOKEN");
        string serverIdsValue = GetRequired("DISCORD_SERVER_IDS");

        string[] values = serverIdsValue.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var serverIds = new List<ulong>();

        foreach (string value in values)
        {
            if (!ulong.TryParse(value, out ulong serverId) || serverId == 0)
            {
                throw new InvalidOperationException(
                    $"DISCORD_SERVER_IDS contains an invalid server ID: '{value}'.");
            }

            if (!serverIds.Contains(serverId))
            {
                serverIds.Add(serverId);
            }
        }

        if (serverIds.Count == 0)
        {
            throw new InvalidOperationException(
                "DISCORD_SERVER_IDS must contain at least one server ID.");
        }

        return new AppConfiguration(
            token,
            serverIds,
            GetDatabaseConnectionString());
    }

    public static string GetDatabaseConnectionString() =>
        Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
        ?? "Data Source=decision-helper.db";

    private static string GetRequired(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required environment variable {name} is missing.");
        }

        return value.Trim();
    }
}
