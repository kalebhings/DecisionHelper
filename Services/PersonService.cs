using DecisionHelper.Models;

namespace DecisionHelper.Services;

public class PersonService
{
  private readonly Dictionary<ulong, Person> _people = [];

  public Person SetNickname(
      ulong discordId,
      string nickname
      )
  {
    string normalizedNickname = nickname.Trim();

    if (string.IsNullOrWhiteSpace(normalizedNickname))
    {
      throw new ArgumentException(
          "Nickname cannot be empty",
          nameof(nickname)
          );
    }

    var person = new Person
    {
      DiscordId = discordId,
      Nickname = normalizedNickname
    };

    _people[discordId] = person;

    return person;
  }

  public Person GetOrCreatePerson(
      ulong discordId,
      string defaultNickname
      )
  {
    if (_people.TryGetValue(discordId, out var person))
    {
      return person;
    }

    person = new Person
    {
      DiscordId = discordId,
      Nickname = defaultNickname
    };

    _people[discordId] = person;

    return person;
  }

  public Person? GetPerson(ulong discordId)
  {
    _people.TryGetValue(discordId, out var person);

    return person;
  }
}
