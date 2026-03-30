using AmazonBestSellersExplorer.Application.Features.Auth.Dtos;
using AmazonBestSellersExplorer.Domain.Entities;

namespace AmazonBestSellersExplorer.Application.Abstractions.Services;

public interface IJwtTokenService
{
    AuthResponse GenerateToken(User user);
}
