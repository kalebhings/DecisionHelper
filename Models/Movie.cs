namespace DecisionHelper.Models;

public class Movie
{
    public int Id { get; set; }

    public required string GuildId { get; set; }

    public required string Title { get; set; }

    public required string NormalizedTitle { get; set; }

    public int? ReleaseYear { get; set; }

    public int AddedByPersonId { get; set; }

    public Person? AddedBy { get; set; }

    public DateTime AddedAtUtc { get; set; }

    public ICollection<MovieTag> MovieTags { get; set; } = [];

    public ICollection<MovieWatchStatus> WatchStatuses { get; set; } = [];
}
