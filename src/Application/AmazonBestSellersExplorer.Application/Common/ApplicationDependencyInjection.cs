using AmazonBestSellersExplorer.Application.Features.Auth.RegisterUser;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AmazonBestSellersExplorer.Application.Common;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly));
        services.AddValidatorsFromAssemblyContaining<RegisterUserCommandHandler>();

        return services;
    }
}
