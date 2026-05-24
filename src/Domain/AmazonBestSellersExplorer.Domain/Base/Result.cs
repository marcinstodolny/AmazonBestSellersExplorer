using System.Diagnostics.CodeAnalysis;

namespace AmazonBestSellersExplorer.Domain.Base;

public class Result
{
    private const string DefaultErrorMessage = "Unknown error";

    protected Result(bool isSuccess, IReadOnlyCollection<string> errors, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        ErrorCode = isSuccess ? null : errorCode;
        Errors = NormalizeErrors(isSuccess, errors);
    }

    public bool IsSuccess { get; }

    public bool IsFailed => !IsSuccess;

    public string? ErrorCode { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public string? FirstError => Errors.FirstOrDefault();

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Fail(string error, string? errorCode = null) => new(false, new[] { error }, errorCode);

    public static Result Fail(IReadOnlyCollection<string> errors, string? errorCode = null) => new(false, errors, errorCode);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    public static Result<TValue> Fail<TValue>(string error, string? errorCode = null) => Result<TValue>.Fail(error, errorCode);

    public static Result<TValue> Fail<TValue>(IReadOnlyCollection<string> errors, string? errorCode = null) => Result<TValue>.Fail(errors, errorCode);

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
    private Result(bool isSuccess, TValue? value, IReadOnlyCollection<string> errors, string? errorCode = null)
        : base(isSuccess, errors, errorCode)
    {
        Value = value;
    }

    public TValue? Value { get; }

    public static Result<TValue> Success(TValue value) => new(true, value, Array.Empty<string>());

    public new static Result<TValue> Fail(string error, string? errorCode = null) => new(false, default, new[] { error }, errorCode);

    public new static Result<TValue> Fail(IReadOnlyCollection<string> errors, string? errorCode = null) => new(false, default, errors, errorCode);

    public bool TryGetValue([NotNullWhen(true)] out TValue? value)
    {
        value = Value;
        return IsSuccess;
    }
}
