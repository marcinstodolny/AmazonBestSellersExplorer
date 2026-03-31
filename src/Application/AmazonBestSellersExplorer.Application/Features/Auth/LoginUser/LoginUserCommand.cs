using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Common;
using AmazonBestSellersExplorer.Application.Features.Auth.Dtos;
using AmazonBestSellersExplorer.Domain.Base;
using AmazonBestSellersExplorer.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AmazonBestSellersExplorer.Application.Features.Auth.LoginUser;

public sealed record LoginUserCommand(
    string Username,
    string Password) : IRequest<Result<AuthResponse>>;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    ILogger<LoginUserCommandHandler> logger,
    IValidator<LoginUserCommand> validator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginUserCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Fail<AuthResponse>(validationResult.Errors.Select(static error => error.ErrorMessage).ToArray());
        }

        var user = await userRepository.GetByUsernameAsync(command.Username, cancellationToken);
        if (user is null)
        {
            return Result.Fail<AuthResponse>(ApplicationMessages.Auth.InvalidCredentials);
        }

        var passwordIsValid = passwordHasher.VerifyPassword(command.Password, user.PasswordHash);
        if (!passwordIsValid)
        {
            return Result.Fail<AuthResponse>(ApplicationMessages.Auth.InvalidCredentials);
        }

        var auditLogResult = AuditLog.Create(
            action: "UserLoggedIn",
            entityType: nameof(User),
            entityId: user.Id.ToString(),
            userId: user.Id);

        if (auditLogResult.IsFailed || !auditLogResult.TryGetValue(out var auditLog))
        {
            return Result.Fail<AuthResponse>(auditLogResult.Errors);
        }

        await auditLogRepository.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResponse = jwtTokenService.GenerateToken(user);

        return Result.Success(authResponse);
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
