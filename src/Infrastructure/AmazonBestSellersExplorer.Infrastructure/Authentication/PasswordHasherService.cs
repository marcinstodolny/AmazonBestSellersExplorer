using AmazonBestSellersExplorer.Application.Features.Auth.Services;
using AmazonBestSellersExplorer.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AmazonBestSellersExplorer.Infrastructure.Authentication;

public sealed class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var verificationResult = _passwordHasher.VerifyHashedPassword(null!, passwordHash, password);

        return verificationResult is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
