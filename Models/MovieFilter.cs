namespace DecisionHelper.Models;

public class MovieFilter
{
    public IReadOnlyCollection<int>? AddedByPersonIds { get; init; }

    public int? WatchStatusPersonId { get; init; }

    public WatchFilter WatchStatus { get; init; }
        = WatchFilter.Any;
}
