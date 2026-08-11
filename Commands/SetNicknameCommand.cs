using DecisionHelper.Discord;
using Discord.WebSocket;
using DecisionHelper.Services;

public class SetNicknameCommand : ICommand
{
    private readonly PersonService _personService;

    public string Name => "setnickname";

    public SetNicknameCommand(PersonService personService)
    {
        _personService = personService;
    }

    public async Task ExecuteAsync(SocketSlashCommand command)
    {
        if (!command.GuildId.HasValue)
        {
            await InteractionResponses.RespondAsync(
                command,
                "Nicknames can only be set in a server.",
                ephemeral: true);
            return;
        }

        var nicknameOption = command.Data.Options
            .FirstOrDefault(option => option.Name == "nickname");

        string? nickname =
            nicknameOption?.Value?.ToString();

        string trimmedNickname = nickname?.Trim() ?? string.Empty;

        if (trimmedNickname.Length is 0 or > InputValidator.MaxNicknameLength ||
            trimmedNickname.Any(char.IsControl))
        {
            await InteractionResponses.RespondAsync(
                command,
                "Provide a nickname of 50 characters or fewer.",
                ephemeral: true);

            return;
        }

        await command.DeferAsync(ephemeral: true);

        var person = await _personService.SetNicknameAsync(
            command.GuildId.Value,
            command.User.Id,
            trimmedNickname);

        await InteractionResponses.CompleteAsync(
            command,
            $"Your nickname is now **{Discord.Format.Sanitize(person.Nickname)}**.");
    }
}
