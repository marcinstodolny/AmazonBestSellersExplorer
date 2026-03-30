using AmazonBestSellersExplorer.Domain.Base;

namespace AmazonBestSellersExplorer.Domain.ValueObjects;

public sealed record Username
{
    public const int MinLength = 5;
    public const int MaxLength = 50;

    private Username(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Username> Create(string value)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add("Username is required.");
        }
        else
        {
            if (value.Length is < MinLength or > MaxLength)
            {
                errors.Add($"Username must be between {MinLength} and {MaxLength} characters.");
            }

            if (!value.All(static character => char.IsAsciiLetterOrDigit(character)))
            {
                errors.Add("Username can contain only ASCII letters and digits.");
            }
        }

        return errors.Count > 0 ? Result.Fail<Username>(errors) : Result.Success(new Username(value));
    }

    public static Username FromPersistence(string value)
    {
        var result = Create(value);

        return result.TryGetValue(out var username)
            ? username
            : throw new InvalidOperationException("Persisted username value is invalid.");
    }

    public override string ToString() => Value;
}
