using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Common;
using AmazonBestSellersExplorer.Application.Features.Bestsellers.Dtos;
using AmazonBestSellersExplorer.Domain.Base;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Bestsellers.GetSoftwareBestSellers;

public sealed record GetSoftwareBestSellersQuery
    : IRequest<Result<IReadOnlyList<BestsellerProductDto>>>;

public sealed class GetSoftwareBestSellersQueryHandler(
    IAmazonBestSellerService amazonBestSellerService)
    : IRequestHandler<GetSoftwareBestSellersQuery, Result<IReadOnlyList<BestsellerProductDto>>>
{
    public async Task<Result<IReadOnlyList<BestsellerProductDto>>> Handle(
        GetSoftwareBestSellersQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var bestSellers = await amazonBestSellerService.GetSoftwareBestSellersAsync(cancellationToken);

            return Result.Success(bestSellers);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Result.Fail<IReadOnlyList<BestsellerProductDto>>(ApplicationMessages.Bestsellers.FailedToRetrieveSoftwareBestSellers);
        }
    }
}
