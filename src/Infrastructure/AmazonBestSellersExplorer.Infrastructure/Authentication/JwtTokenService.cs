using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AmazonBestSellersExplorer.Application.Features.Auth.Contracts;
using AmazonBestSellersExplorer.Application.Features.Auth.Dtos;
using AmazonBestSellersExplorer.Application.Features.Auth.Services;
using AmazonBestSellersExplorer.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AmazonBestSellersExplorer.Infrastructure.Authentication;

public sealed class JwtTokenService(IOptions<JwtOptions> configuredOptions) : IJwtTokenService
{
    public AuthResponse GenerateToken(User user, JwtOptions options)
    {
        var jwtOptions = ResolveOptions(options);
        ValidateOptions(jwtOptions);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username.Value)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse(accessToken, expiresAtUtc);
    }

    private JwtOptions ResolveOptions(JwtOptions options)
    {
        var fallbackOptions = configuredOptions.Value;

        return string.IsNullOrWhiteSpace(options.Issuer)
            || string.IsNullOrWhiteSpace(options.Audience)
            || string.IsNullOrWhiteSpace(options.SecretKey)
            || options.ExpirationMinutes <= 0
            ? fallbackOptions
            : options;
    }

    private static void ValidateOptions(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new InvalidOperationException("JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("JWT audience is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            throw new InvalidOperationException("JWT secret key is not configured.");
        }

        if (options.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT expiration must be greater than zero.");
        }
    }
}
