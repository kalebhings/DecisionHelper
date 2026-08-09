using DecisionHelper.Data;
using DecisionHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Services;

public class PersonService
{
    private readonly IDbContextFactory<DecisionHelperDbContext>
        _dbContextFactory;

    public PersonService(
        IDbContextFactory<DecisionHelperDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Person> GetOrCreatePersonAsync(
        ulong discordId,
        string defaultNickname)
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        string discordUserId = discordId.ToString();

        var person = await db.People
            .SingleOrDefaultAsync(
                person =>
                    person.DiscordUserId == discordUserId);

        if (person is not null)
        {
            return person;
        }

        person = new Person
        {
            DiscordUserId = discordUserId,
            Nickname = defaultNickname.Trim()
        };

        db.People.Add(person);

        await db.SaveChangesAsync();

        return person;
    }

    public async Task<Person?> GetPersonAsync(
        ulong discordId)
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        string discordUserId = discordId.ToString();

        return await db.People
            .SingleOrDefaultAsync(
                person =>
                    person.DiscordUserId == discordUserId);
    }

    public async Task<Person> SetNicknameAsync(
        ulong discordId,
        string nickname)
    {
        string normalizedNickname = nickname.Trim();

        if (string.IsNullOrWhiteSpace(normalizedNickname))
        {
            throw new ArgumentException(
                "Nickname cannot be empty.",
                nameof(nickname));
        }

        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        string discordUserId = discordId.ToString();

        var person = await db.People
            .SingleOrDefaultAsync(
                person =>
                    person.DiscordUserId == discordUserId);

        if (person is null)
        {
            person = new Person
            {
                DiscordUserId = discordUserId,
                Nickname = normalizedNickname
            };

            db.People.Add(person);
        }
        else
        {
            person.Nickname = normalizedNickname;
        }

        await db.SaveChangesAsync();

        return person;
    }
}