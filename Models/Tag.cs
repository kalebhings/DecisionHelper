namespace DecisionHelper.Models;

public class Tag
{
    public int Id { get; set; }

    public required string GuildId { get; set; }

    public required string Name { get; set; }

    public required string NormalizedName { get; set; }

    public TagKind Kind { get; set; }

    public ICollection<MovieTag> MovieTags { get; set; } = [];
}
