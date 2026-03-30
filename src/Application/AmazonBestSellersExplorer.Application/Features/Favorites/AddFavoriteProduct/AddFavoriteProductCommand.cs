using AmazonBestSellersExplorer.Application.Abstractions.Authentication;
using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Common;
using AmazonBestSellersExplorer.Domain.Base;
using AmazonBestSellersExplorer.Domain.Entities;
using FluentValidation;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Favorites.AddFavoriteProduct;

public sealed record AddFavoriteProductCommand(
    string AmazonProductId,
    string Title,
    decimal? Price,
    double? Rating,
    string ProductUrl,
    string? ImageUrl) : IRequest<Result>;

public sealed class AddFavoriteProductCommandHandler(
    ICurrentUserContext currentUserContext,
    IFavoriteProductRepository favoriteProductRepository,
    IAuditLogRepository auditLogRepository,
    IValidator<AddFavoriteProductCommand> validator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddFavoriteProductCommand, Result>
{
    public async Task<Result> Handle(
        AddFavoriteProductCommand command,
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
        var title = command.Title.Trim();
        var productUrl = command.ProductUrl.Trim();
        var imageUrl = string.IsNullOrWhiteSpace(command.ImageUrl)
            ? null
            : command.ImageUrl.Trim();

        var existingFavoriteProduct = await favoriteProductRepository.GetByUserIdAndAmazonProductIdAsync(
            userId,
            amazonProductId,
            cancellationToken);

        if (existingFavoriteProduct is not null)
        {
            return Result.Fail(ApplicationMessages.Favorites.FavoriteProductAlreadyExists);
        }

        var favoriteProductResult = FavoriteProduct.Create(
            userId,
            amazonProductId,
            title,
            command.Price,
            command.Rating,
            productUrl,
            imageUrl);

        if (favoriteProductResult.IsFailed || !favoriteProductResult.TryGetValue(out var favoriteProduct))
        {
            return Result.Fail(favoriteProductResult.Errors);
        }

        await favoriteProductRepository.AddAsync(favoriteProduct, cancellationToken);

        var auditLogResult = AuditLog.Create(
            action: "FavoriteProductAdded",
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

public sealed class AddFavoriteProductCommandValidator : AbstractValidator<AddFavoriteProductCommand>
{
    public AddFavoriteProductCommandValidator()
    {
        RuleFor(command => command.AmazonProductId)
            .NotEmpty()
            .WithMessage(ApplicationMessages.Favorites.AmazonProductIdRequired);

        RuleFor(command => command.Title)
            .NotEmpty()
            .WithMessage(ApplicationMessages.Favorites.TitleRequired);

        RuleFor(command => command.ProductUrl)
            .NotEmpty()
            .WithMessage(ApplicationMessages.Favorites.ProductUrlRequired);
    }
}
