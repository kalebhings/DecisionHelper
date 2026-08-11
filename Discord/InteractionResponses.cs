using Discord;
using Discord.WebSocket;

namespace DecisionHelper.Discord;

public static class InteractionResponses
{
    public static Task RespondAsync(
        SocketInteraction interaction,
        string content,
        bool ephemeral = false)
    {
        return interaction.RespondAsync(
            content,
            ephemeral: ephemeral,
            allowedMentions: AllowedMentions.None);
    }

    public static Task CompleteAsync(
        SocketInteraction interaction,
        string content)
    {
        return interaction.ModifyOriginalResponseAsync(properties =>
        {
            properties.Content = content;
            properties.AllowedMentions = AllowedMentions.None;
        });
    }

    public static Task FollowupAsync(
        SocketInteraction interaction,
        string content)
    {
        return interaction.FollowupAsync(
            content,
            allowedMentions: AllowedMentions.None);
    }
}
