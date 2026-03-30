using Microsoft.Extensions.DependencyInjection;

namespace AmazonBestSellersExplorer.Application.Common;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
