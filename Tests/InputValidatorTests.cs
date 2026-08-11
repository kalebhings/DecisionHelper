using DecisionHelper.Services;

namespace DecisionHelper.Tests;

public class InputValidatorTests
{
    [Fact]
    public void MovieTitle_AllowsSqlCharactersWithoutExecutingThem()
    {
        const string input = "Robert'); DROP TABLE Movies;--";

        string result = InputValidator.MovieTitle(input);

        Assert.Equal(input, result);
    }

    [Theory]
    [InlineData("Movie\nTitle")]
    [InlineData("Movie\0Title")]
    public void MovieTitle_RejectsControlCharacters(string input)
    {
        Assert.Throws<ArgumentException>(() =>
            InputValidator.MovieTitle(input));
    }
}
