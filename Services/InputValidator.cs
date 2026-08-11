namespace DecisionHelper.Services;

public static class InputValidator
{
    public const int MaxMovieTitleLength = 200;
    public const int MaxNicknameLength = 50;

    public static string MovieTitle(string value) =>
        Validate(value, MaxMovieTitleLength, "Movie title", nameof(value));

    public static string Nickname(string value) =>
        Validate(value, MaxNicknameLength, "Nickname", nameof(value));

    private static string Validate(
        string value,
        int maxLength,
        string displayName,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value);

        string trimmed = value.Trim();

        if (trimmed.Length == 0)
        {
            throw new ArgumentException(
                $"{displayName} cannot be empty.",
                parameterName);
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException(
                $"{displayName} cannot exceed {maxLength} characters.",
                parameterName);
        }

        if (trimmed.Any(char.IsControl))
        {
            throw new ArgumentException(
                $"{displayName} cannot contain control characters.",
                parameterName);
        }

        return trimmed;
    }
}
