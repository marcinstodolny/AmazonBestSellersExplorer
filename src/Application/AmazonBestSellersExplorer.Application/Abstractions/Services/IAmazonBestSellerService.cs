using AmazonBestSellersExplorer.Application.Features.Bestsellers.Dtos;

namespace AmazonBestSellersExplorer.Application.Abstractions.Services;

public interface IAmazonBestSellerService
{
    Task<IReadOnlyList<BestsellerProductDto>> GetSoftwareBestSellersAsync(CancellationToken cancellationToken);
}
