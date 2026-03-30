using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Features.Auth.Dtos;
using AmazonBestSellersExplorer.Domain.Base;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Auth.LoginUser;

public sealed record LoginUserCommand(
    string Username,
    string Password) : IRequest<Result<AuthResponse>>;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginUserCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(command);
        if (validationErrors.Count > 0)
        {
            return Result.Fail<AuthResponse>(validationErrors);
        }

        var user = await userRepository.GetByUsernameAsync(command.Username, cancellationToken);
        if (user is null)
        {
            return Result.Fail<AuthResponse>("Invalid username or password.");
        }

        var passwordIsValid = passwordHasher.VerifyPassword(command.Password, user.PasswordHash);
        if (!passwordIsValid)
        {
            return Result.Fail<AuthResponse>("Invalid username or password.");
        }

        var authResponse = jwtTokenService.GenerateToken(user);

        return Result.Success(authResponse);
    }

    private static IReadOnlyCollection<string> Validate(LoginUserCommand command)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(command.Username))
        {
            errors.Add("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors.Add("Password is required.");
        }

        return errors;
    }
}
