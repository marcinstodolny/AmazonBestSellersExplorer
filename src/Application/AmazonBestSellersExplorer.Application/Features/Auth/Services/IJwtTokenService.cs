using AmazonBestSellersExplorer.Application.Features.Auth.Dtos;
using AmazonBestSellersExplorer.Domain.Entities;

namespace AmazonBestSellersExplorer.Application.Features.Auth.Services;

public interface IJwtTokenService
{
    AuthResponse GenerateToken(User user);
}
