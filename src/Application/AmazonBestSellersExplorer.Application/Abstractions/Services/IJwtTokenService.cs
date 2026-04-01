using AmazonBestSellersExplorer.Application.Features.Auth.Contracts;
using AmazonBestSellersExplorer.Domain.Entities;

namespace AmazonBestSellersExplorer.Application.Abstractions.Services;

public interface IJwtTokenService
{
    AuthResponse GenerateToken(User user);
}
