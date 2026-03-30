using AmazonBestSellersExplorer.Application.Abstractions.Persistence;
using AmazonBestSellersExplorer.Application.Abstractions.Services;
using AmazonBestSellersExplorer.Application.Features.Auth.Contracts;
using AmazonBestSellersExplorer.Infrastructure.Authentication;
using AmazonBestSellersExplorer.Infrastructure.Integrations.RapidApi;
using AmazonBestSellersExplorer.Infrastructure.Persistence;
using AmazonBestSellersExplorer.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AmazonBestSellersExplorer.Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<RapidApiOptions>(configuration.GetSection(RapidApiOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddHttpClient<IAmazonBestSellerService, RapidApiAmazonBestSellerService>((serviceProvider, client) =>
        {
            var rapidApiOptions = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<RapidApiOptions>>()
                .Value;

            if (string.IsNullOrWhiteSpace(rapidApiOptions.BaseUrl))
            {
                throw new InvalidOperationException("RapidAPI base URL is not configured.");
            }

            if (string.IsNullOrWhiteSpace(rapidApiOptions.ApiKey))
            {
                throw new InvalidOperationException("RapidAPI API key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(rapidApiOptions.ApiHost))
            {
                throw new InvalidOperationException("RapidAPI API host is not configured.");
            }

            client.BaseAddress = new Uri(rapidApiOptions.BaseUrl);
            client.DefaultRequestHeaders.Add("x-rapidapi-key", rapidApiOptions.ApiKey);
            client.DefaultRequestHeaders.Add("x-rapidapi-host", rapidApiOptions.ApiHost);
        });

        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFavoriteProductRepository, FavoriteProductRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());

        return services;
    }
}
