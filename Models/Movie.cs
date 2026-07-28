namespace DecisionHelper.Models;

public class Movie
{
  public required string Name { get; init; }

  public required Person AddedBy { get; init; }
}
