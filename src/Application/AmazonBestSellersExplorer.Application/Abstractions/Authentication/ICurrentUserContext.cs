namespace AmazonBestSellersExplorer.Application.Abstractions.Authentication;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    string? Username { get; }
}
