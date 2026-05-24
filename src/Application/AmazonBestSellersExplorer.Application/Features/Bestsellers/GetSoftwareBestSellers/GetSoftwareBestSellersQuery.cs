using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Features.Bestsellers.Contracts;
using AmazonBestSellersExplorer.Application.Common;
using AmazonBestSellersExplorer.Domain.Base;
using MediatR;

namespace AmazonBestSellersExplorer.Application.Features.Bestsellers.GetSoftwareBestSellers;

public sealed record GetSoftwareBestSellersQuery
    : IRequest<Result<IReadOnlyList<BestsellerProductResponse>>>;

public sealed class GetSoftwareBestSellersQueryHandler(
    IAmazonBestSellerService amazonBestSellerService)
    : IRequestHandler<GetSoftwareBestSellersQuery, Result<IReadOnlyList<BestsellerProductResponse>>>
{
    public async Task<Result<IReadOnlyList<BestsellerProductResponse>>> Handle(
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
        catch (AmazonBestSellerServiceConfigurationException)
        {
            return Result.Fail<IReadOnlyList<BestsellerProductResponse>>(ApplicationMessages.Bestsellers.ServiceUnavailable);
        }
        catch
        {
            return Result.Fail<IReadOnlyList<BestsellerProductResponse>>(ApplicationMessages.Bestsellers.FailedToRetrieveSoftwareBestSellers);
        }
    }
}
