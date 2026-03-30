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
    IFavoriteProductRepository favoriteProductRepository,
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

        var favoriteProduct = await favoriteProductRepository.GetByUserIdAndAmazonProductIdAsync(
            userId,
            amazonProductId,
            cancellationToken);

        if (favoriteProduct is null)
        {
            return Result.Fail(ApplicationMessages.Favorites.FavoriteProductDoesNotExist);
        }

        favoriteProductRepository.Remove(favoriteProduct);

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
