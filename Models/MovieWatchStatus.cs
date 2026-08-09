namespace DecisionHelper.Models;

public class MovieWatchStatus
{
    public int MovieId { get; set; }

    public required Movie Movie { get; set; }

    public int PersonId { get; set; }

    public required Person Person { get; set; }

    public bool HasSeen { get; set; }

    public DateTime? WatchedAtUtc { get; set; }
}
