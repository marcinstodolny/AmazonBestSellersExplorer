using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AmazonBestSellersExplorer.Application.Common;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly));

        return services;
    }
}
