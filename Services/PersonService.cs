using DecisionHelper.Data;
using DecisionHelper.Models;
using Microsoft.Data.Sqlite;
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
        ulong guildId,
        ulong discordId,
        string defaultNickname)
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        string guildUserId = guildId.ToString();
        string discordUserId = discordId.ToString();

        var person = await db.People
            .SingleOrDefaultAsync(
                person =>
                    person.GuildId == guildUserId &&
                    person.DiscordUserId == discordUserId);

        if (person is not null)
        {
            return person;
        }

        person = new Person
        {
            GuildId = guildUserId,
            DiscordUserId = discordUserId,
            Nickname = InputValidator.Nickname(defaultNickname)
        };

        db.People.Add(person);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            db.Entry(person).State = EntityState.Detached;

            return await db.People.SingleAsync(candidate =>
                candidate.GuildId == guildUserId &&
                candidate.DiscordUserId == discordUserId);
        }

        return person;
    }

    public async Task<Person?> GetPersonAsync(
        ulong guildId,
        ulong discordId)
    {
        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        string discordUserId = discordId.ToString();
        string guildUserId = guildId.ToString();

        return await db.People
            .SingleOrDefaultAsync(
                person =>
                    person.GuildId == guildUserId &&
                    person.DiscordUserId == discordUserId);
    }

    public async Task<IReadOnlyList<int>> GetPersonIdsByNicknameAsync(
        ulong guildId,
        string nickname)
    {
        string validatedNickname = InputValidator.Nickname(nickname);
        string guildUserId = guildId.ToString();

        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        return await db.People
            .AsNoTracking()
            .Where(person =>
                person.GuildId == guildUserId &&
                EF.Functions.Collate(person.Nickname, "NOCASE") ==
                    validatedNickname)
            .Select(person => person.Id)
            .ToListAsync();
    }

    public async Task<Person> SetNicknameAsync(
        ulong guildId,
        ulong discordId,
        string nickname)
    {
        string normalizedNickname = InputValidator.Nickname(nickname);

        await using var db =
            await _dbContextFactory.CreateDbContextAsync();

        string discordUserId = discordId.ToString();
        string guildUserId = guildId.ToString();

        var person = await db.People
            .SingleOrDefaultAsync(
                person =>
                    person.GuildId == guildUserId &&
                    person.DiscordUserId == discordUserId);

        if (person is null)
        {
            person = new Person
            {
                GuildId = guildUserId,
                DiscordUserId = discordUserId,
                Nickname = normalizedNickname
            };

            db.People.Add(person);
        }
        else
        {
            person.Nickname = normalizedNickname;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            db.Entry(person).State = EntityState.Detached;
            person = await db.People.SingleAsync(candidate =>
                candidate.GuildId == guildUserId &&
                candidate.DiscordUserId == discordUserId);
            person.Nickname = normalizedNickname;
            await db.SaveChangesAsync();
        }

        return person;
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqliteException
        {
            SqliteExtendedErrorCode: 1555 or 2067
        };
    }
}
