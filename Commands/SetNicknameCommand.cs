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
        var nicknameOption = command.Data.Options
            .FirstOrDefault(option => option.Name == "nickname");

        string? nickname =
            nicknameOption?.Value?.ToString();

        if (string.IsNullOrWhiteSpace(nickname))
        {
            await command.RespondAsync(
                "Please provide a nickname.",
                ephemeral: true
            );

            return;
        }

        var person = await _personService.SetNicknameAsync(
            command.User.Id,
            nickname
        );

        await command.RespondAsync(
            $"Your nickname is now **{person.Nickname}**.",
            ephemeral: true
        );
    }
}
