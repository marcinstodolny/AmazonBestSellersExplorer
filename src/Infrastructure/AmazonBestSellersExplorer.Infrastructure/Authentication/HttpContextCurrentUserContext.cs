using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AmazonBestSellersExplorer.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace AmazonBestSellersExplorer.Infrastructure.Authentication;

public sealed class HttpContextCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            if (!IsAuthenticated)
            {
                return null;
            }

            var rawUserId = GetClaimValue(JwtRegisteredClaimNames.Sub)
                ?? GetClaimValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(rawUserId, out var userId)
                ? userId
                : null;
        }
    }

    public string? Username => !IsAuthenticated
        ? null
        : GetClaimValue(JwtRegisteredClaimNames.UniqueName)
            ?? GetClaimValue(ClaimTypes.Name);

    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    private string? GetClaimValue(string claimType) =>
        User?.FindFirst(claimType)?.Value;
}
