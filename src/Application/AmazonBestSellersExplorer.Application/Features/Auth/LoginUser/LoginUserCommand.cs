using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Common;
using AmazonBestSellersExplorer.Application.Features.Auth.Contracts;
using AmazonBestSellersExplorer.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Auth.LoginUser;

public sealed record LoginUserCommand(
    string Username,
    string Password) : IRequest<AuthResult>;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IValidator<LoginUserCommand> validator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginUserCommand, AuthResult>
{
    public async Task<AuthResult> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return AuthResult.ValidationFailed(validationResult.Errors.Select(static error => error.ErrorMessage).ToArray());
        }

        var user = await userRepository.GetByUsernameAsync(command.Username, cancellationToken);
        if (user is null)
        {
            return AuthResult.InvalidCredentials();
        }

        var passwordIsValid = passwordHasher.VerifyPassword(command.Password, user.PasswordHash);
        if (!passwordIsValid)
        {
            return AuthResult.InvalidCredentials();
        }

        var auditLogResult = AuditLog.Create(
            action: "UserLoggedIn",
            entityType: nameof(User),
            entityId: user.Id.ToString(),
            userId: user.Id);

        if (auditLogResult.IsFailed || !auditLogResult.TryGetValue(out var auditLog))
        {
            return AuthResult.ValidationFailed(auditLogResult.Errors);
        }

        await auditLogRepository.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResponse = jwtTokenService.GenerateToken(user);

        return AuthResult.Success(authResponse);
    }
}

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .WithMessage(ApplicationMessages.Auth.UsernameRequired);

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage(ApplicationMessages.Auth.PasswordRequired);
    }
}
