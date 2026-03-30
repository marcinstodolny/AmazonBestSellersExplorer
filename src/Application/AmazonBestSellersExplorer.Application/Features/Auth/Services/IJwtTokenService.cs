using AmazonBestSellersExplorer.Application.Features.Auth.Dtos;
using AmazonBestSellersExplorer.Application.Features.Auth.Contracts;
using AmazonBestSellersExplorer.Domain.Entities;

namespace AmazonBestSellersExplorer.Application.Features.Auth.Services;

public interface IJwtTokenService
{
    AuthResponse GenerateToken(User user, JwtOptions options);
}
