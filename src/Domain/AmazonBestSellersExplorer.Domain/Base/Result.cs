using System.Diagnostics.CodeAnalysis;

namespace AmazonBestSellersExplorer.Domain.Base;

public class Result
{
    private const string DefaultErrorMessage = "Unknown error";

    protected Result(bool isSuccess, IReadOnlyCollection<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = NormalizeErrors(isSuccess, errors);
    }

    public bool IsSuccess { get; }

    public bool IsFailed => !IsSuccess;

    public IReadOnlyCollection<string> Errors { get; }

    public string? FirstError => Errors.FirstOrDefault();

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Fail(string error) => new(false, new[] { error });

    public static Result Fail(IReadOnlyCollection<string> errors) => new(false, errors);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    public static Result<TValue> Fail<TValue>(string error) => Result<TValue>.Fail(error);

    public static Result<TValue> Fail<TValue>(IReadOnlyCollection<string> errors) => Result<TValue>.Fail(errors);

    private static IReadOnlyCollection<string> NormalizeErrors(bool isSuccess, IReadOnlyCollection<string>? errors)
    {
        if (isSuccess)
        {
            return Array.Empty<string>();
        }

        if (errors is null || errors.Count == 0)
        {
            return new[] { DefaultErrorMessage };
        }

        var normalizedErrors = errors
            .Where(static error => !string.IsNullOrWhiteSpace(error))
            .Distinct()
            .ToArray();

        return normalizedErrors.Length > 0
            ? normalizedErrors
            : new[] { DefaultErrorMessage };
    }
}

public sealed class Result<TValue> : Result
{
    private Result(bool isSuccess, TValue? value, IReadOnlyCollection<string> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public TValue? Value { get; }

    public static Result<TValue> Success(TValue value) => new(true, value, Array.Empty<string>());

    public new static Result<TValue> Fail(string error) => new(false, default, new[] { error });

    public new static Result<TValue> Fail(IReadOnlyCollection<string> errors) => new(false, default, errors);

    public bool TryGetValue([NotNullWhen(true)] out TValue? value)
    {
        value = Value;
        return IsSuccess;
    }
}
