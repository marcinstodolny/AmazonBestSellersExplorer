using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Features.Auth.Dtos;
using AmazonBestSellersExplorer.Domain.Base;
using AmazonBestSellersExplorer.Domain.Entities;
using AmazonBestSellersExplorer.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Auth.RegisterUser;

public sealed record RegisterUserCommand(
    string Username,
    string Password) : IRequest<Result<AuthResponse>>;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IValidator<RegisterUserCommand> validator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{
    private const int MinimumPasswordLength = 8;

    public async Task<Result<AuthResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Fail<AuthResponse>(validationResult.Errors.Select(static error => error.ErrorMessage).ToArray());
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
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private const int MinimumPasswordLength = 8;

    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Username)
            .Custom((username, context) =>
            {
                var usernameResult = Username.Create(username);
                if (usernameResult.IsSuccess)
                {
                    return;
                }

                foreach (var error in usernameResult.Errors)
                {
                    context.AddFailure(error);
                }
            });

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .DependentRules(() =>
            {
                RuleFor(command => command.Password)
                    .MinimumLength(MinimumPasswordLength)
                    .WithMessage($"Password must be at least {MinimumPasswordLength} characters long.");

                RuleFor(command => command.Password)
                    .Must(static password => password.Any(char.IsUpper))
                    .WithMessage("Password must contain at least one uppercase letter.");

                RuleFor(command => command.Password)
                    .Must(static password => password.Any(char.IsLower))
                    .WithMessage("Password must contain at least one lowercase letter.");

                RuleFor(command => command.Password)
                    .Must(static password => password.Any(char.IsDigit))
                    .WithMessage("Password must contain at least one digit.");
            });
    }
}
