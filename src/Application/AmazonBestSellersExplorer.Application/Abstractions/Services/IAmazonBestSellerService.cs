using AmazonBestSellersExplorer.Application.Features.Bestsellers.Contracts;

namespace AmazonBestSellersExplorer.Application.Abstractions.Services;

public interface IAmazonBestSellerService
{
    Task<IReadOnlyList<BestsellerProductResponse>> GetSoftwareBestSellersAsync(CancellationToken cancellationToken);
}
