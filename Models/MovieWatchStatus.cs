namespace DecisionHelper.Models;

public class MovieWatchStatus
{
    public int MovieId { get; set; }

    public Movie? Movie { get; set; }

    public int PersonId { get; set; }

    public Person? Person { get; set; }

    public bool HasSeen { get; set; }

    public DateTime? WatchedAtUtc { get; set; }
}