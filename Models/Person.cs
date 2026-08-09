namespace DecisionHelper.Models;

public class Person
{
    public int Id { get; set; }

    public required string DiscordUserId { get; set; }

    public required string Nickname { get; set; }

    public ICollection<Movie> MoviesAdded { get; set; } = [];

    public ICollection<MovieWatchStatus> MovieWatchStatuses { get; set; } = [];
}