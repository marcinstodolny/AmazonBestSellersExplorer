using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Common;
using AmazonBestSellersExplorer.Application.Features.Auth.Contracts;
using AmazonBestSellersExplorer.Domain.Base;
using AmazonBestSellersExplorer.Domain.Entities;
using AmazonBestSellersExplorer.Domain.Rules;
using AmazonBestSellersExplorer.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Auth.RegisterUser;

public sealed record RegisterUserCommand(
    string Username,
    string Password) : IRequest<Result<AuthResponse>>;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IValidator<RegisterUserCommand> validator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{

    public async Task<Result<AuthResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Fail<AuthResponse>(
                validationResult.Errors.Select(static error => error.ErrorMessage).ToArray(),
                AuthErrorCode.ValidationFailed.ToString());
        }

        var usernameExists = await userRepository.ExistsByUsernameAsync(command.Username, cancellationToken);
        if (usernameExists)
        {
            return Result.Fail<AuthResponse>(
                ApplicationMessages.Auth.UsernameAlreadyTaken,
                AuthErrorCode.UsernameAlreadyTaken.ToString());
        }

        var passwordHash = passwordHasher.HashPassword(command.Password);
        var userResult = User.Create(command.Username, passwordHash);
        if (userResult.IsFailed || !userResult.TryGetValue(out var user))
        {
            return Result.Fail<AuthResponse>(
                userResult.Errors,
                AuthErrorCode.ValidationFailed.ToString());
        }

        await userRepository.AddAsync(user, cancellationToken);

        var auditLogResult = AuditLog.Create(
            action: "UserRegistered",
            entityType: nameof(User),
            entityId: user.Id.ToString(),
            userId: user.Id);

        if (auditLogResult.IsFailed || !auditLogResult.TryGetValue(out var auditLog))
        {
            return Result.Fail<AuthResponse>(
                auditLogResult.Errors,
                AuthErrorCode.ValidationFailed.ToString());
        }

        await auditLogRepository.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResponse = jwtTokenService.GenerateToken(user);

        return Result.Success(authResponse);
    }
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
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
            .WithMessage(ApplicationMessages.Auth.PasswordRequired)
            .DependentRules(() =>
            {
                RuleFor(command => command.Password)
                    .MinimumLength(UserRules.PasswordMinimumLength)
                    .WithMessage(ApplicationMessages.Auth.PasswordMinimumLength(UserRules.PasswordMinimumLength));

                RuleFor(command => command.Password)
                    .Must(static password => password.Any(char.IsUpper))
                    .WithMessage(ApplicationMessages.Auth.PasswordUppercaseRequired);

                RuleFor(command => command.Password)
                    .Must(static password => password.Any(char.IsLower))
                    .WithMessage(ApplicationMessages.Auth.PasswordLowercaseRequired);

                RuleFor(command => command.Password)
                    .Must(static password => password.Any(char.IsDigit))
                    .WithMessage(ApplicationMessages.Auth.PasswordDigitRequired);
            });
    }
}
