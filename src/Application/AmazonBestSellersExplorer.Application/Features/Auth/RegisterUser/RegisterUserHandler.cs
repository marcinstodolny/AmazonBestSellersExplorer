using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Features.Auth.Dtos;
using AmazonBestSellersExplorer.Application.Features.Auth.Services;
using AmazonBestSellersExplorer.Domain.Base;
using AmazonBestSellersExplorer.Domain.Entities;
using AmazonBestSellersExplorer.Domain.ValueObjects;

namespace AmazonBestSellersExplorer.Application.Features.Auth.RegisterUser;

public sealed class RegisterUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork)
{
    private const int MinimumPasswordLength = 8;

    public async Task<Result<AuthResponse>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(command);
        if (validationErrors.Count > 0)
        {
            return Result.Fail<AuthResponse>(validationErrors);
        }

        var usernameExists = await userRepository.ExistsByUsernameAsync(command.Username, cancellationToken);
        if (usernameExists)
        {
            return Result.Fail<AuthResponse>("Username is already taken.");
        }

        var passwordHash = passwordHasher.HashPassword(command.Password);
        var userResult = User.Create(command.Username, passwordHash);
        if (userResult.IsFailed || !userResult.TryGetValue(out var user))
        {
            return Result.Fail<AuthResponse>(userResult.Errors);
        }

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResponse = jwtTokenService.GenerateToken(user);

        return Result.Success(authResponse);
    }

    private static IReadOnlyCollection<string> Validate(RegisterUserCommand command)
    {
        var errors = new List<string>();

        var usernameValidationResult = Username.Create(command.Username);
        if (usernameValidationResult.IsFailed)
        {
            errors.AddRange(usernameValidationResult.Errors);
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors.Add("Password is required.");
            return errors;
        }

        if (command.Password.Length < MinimumPasswordLength)
        {
            errors.Add($"Password must be at least {MinimumPasswordLength} characters long.");
        }

        if (!command.Password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (!command.Password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        if (!command.Password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit.");
        }

        return errors;
    }
}
