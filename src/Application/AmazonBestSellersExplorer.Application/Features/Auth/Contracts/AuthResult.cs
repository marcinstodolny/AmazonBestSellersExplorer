using System.Diagnostics.CodeAnalysis;
using AmazonBestSellersExplorer.Application.Common;

namespace AmazonBestSellersExplorer.Application.Features.Auth.Contracts;

public sealed class AuthResult
{
    private const string DefaultErrorMessage = "Unknown error";

    private AuthResult(
        bool isSuccess,
        AuthResponse? value,
        AuthErrorCode? errorCode,
        IReadOnlyCollection<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorCode = errorCode;
        Errors = NormalizeErrors(isSuccess, errors);
    }

    public bool IsSuccess { get; }

    public bool IsFailed => !IsSuccess;

    public AuthResponse? Value { get; }

    public AuthErrorCode? ErrorCode { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static AuthResult Success(AuthResponse response) =>
        new(true, response, null, Array.Empty<string>());

    public static AuthResult ValidationFailed(IReadOnlyCollection<string> errors) =>
        new(false, null, AuthErrorCode.ValidationFailed, errors);

    public static AuthResult InvalidCredentials() =>
        new(false, null, AuthErrorCode.InvalidCredentials, [ApplicationMessages.Auth.InvalidCredentials]);

    public static AuthResult UsernameAlreadyTaken() =>
        new(false, null, AuthErrorCode.UsernameAlreadyTaken, [ApplicationMessages.Auth.UsernameAlreadyTaken]);

    public bool TryGetValue([NotNullWhen(true)] out AuthResponse? value)
    {
        value = Value;
        return IsSuccess;
    }

    private static IReadOnlyCollection<string> NormalizeErrors(bool isSuccess, IReadOnlyCollection<string>? errors)
    {
        if (isSuccess)
        {
            return Array.Empty<string>();
        }

        if (errors is null || errors.Count == 0)
        {
            return [DefaultErrorMessage];
        }

        var normalizedErrors = errors
            .Where(static error => !string.IsNullOrWhiteSpace(error))
            .Distinct()
            .ToArray();

        return normalizedErrors.Length > 0
            ? normalizedErrors
            : [DefaultErrorMessage];
    }
}
