using AmazonBestSellersExplorer.Application.Abstractions.Authentication;
using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Common;
using AmazonBestSellersExplorer.Domain.Base;
using AmazonBestSellersExplorer.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Favorites.RemoveFavoriteProduct;

public sealed record RemoveFavoriteProductCommand(
    string AmazonProductId) : IRequest<Result>;

public sealed class RemoveFavoriteProductCommandHandler(
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    IValidator<RemoveFavoriteProductCommand> validator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveFavoriteProductCommand, Result>
{
    public async Task<Result> Handle(
        RemoveFavoriteProductCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.Select(static error => error.ErrorMessage).ToArray());
        }

        if (!currentUserContext.IsAuthenticated || currentUserContext.UserId is null)
        {
            return Result.Fail(ApplicationMessages.Favorites.UserNotAuthenticated);
        }

        var userId = currentUserContext.UserId.Value;
        var amazonProductId = command.AmazonProductId.Trim();

        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            return Result.Fail(ApplicationMessages.Favorites.UserNotAuthenticated);
        }

        var favoriteProduct = user.FavoriteProducts.FirstOrDefault(product =>
            string.Equals(product.AmazonProductId, amazonProductId, StringComparison.OrdinalIgnoreCase));

        if (favoriteProduct is null)
        {
            return Result.Fail(ApplicationMessages.Favorites.FavoriteProductDoesNotExist);
        }

        var removeFavoriteProductResult = user.RemoveFavoriteProduct(amazonProductId);
        if (removeFavoriteProductResult.IsFailed)
        {
            return Result.Fail(removeFavoriteProductResult.Errors);
        }

        var auditLogResult = AuditLog.Create(
            action: "FavoriteProductRemoved",
            entityType: nameof(FavoriteProduct),
            entityId: favoriteProduct.Id.ToString(),
            userId: userId);

        if (auditLogResult.IsFailed || !auditLogResult.TryGetValue(out var auditLog))
        {
            return Result.Fail(auditLogResult.Errors);
        }

        await auditLogRepository.AddAsync(auditLog, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class RemoveFavoriteProductCommandValidator : AbstractValidator<RemoveFavoriteProductCommand>
{
    public RemoveFavoriteProductCommandValidator()
    {
        RuleFor(command => command.AmazonProductId)
            .NotEmpty()
            .WithMessage(ApplicationMessages.Favorites.AmazonProductIdRequired);
    }
}
