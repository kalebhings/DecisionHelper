namespace DecisionHelper.Models;

public class MovieTag
{
    public int MovieId { get; set; }

    public required Movie Movie { get; set; }

    public int TagId { get; set; }

    public required Tag Tag { get; set; }
}
