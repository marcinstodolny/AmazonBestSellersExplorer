namespace AmazonBestSellersExplorer.Application.Features.Auth.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}
