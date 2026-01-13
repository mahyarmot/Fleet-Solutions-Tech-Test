using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NhsTechTest.Domain.Repositories;
using NhsTechTest.Infrastructure.Configuration;
using NhsTechTest.Infrastructure.Persistence;

namespace NhsTechTest.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind CosmosDB settings
        var cosmosDbSettings = new CosmosDbSettings();
        configuration.GetSection(CosmosDbSettings.SectionName).Bind(cosmosDbSettings);
        services.AddSingleton(cosmosDbSettings);

        // Register CosmosClient
        services.AddSingleton<CosmosClient>(sp =>
        {
            var settings = sp.GetRequiredService<CosmosDbSettings>();

            return new CosmosClient(
                accountEndpoint: settings.AccountEndpoint,
                authKeyOrResourceToken: settings.AccountKey,
                clientOptions: new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    },
                    ConnectionMode = ConnectionMode.Direct,
                    MaxRetryAttemptsOnRateLimitedRequests = 3,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(10)
                });
        });

        // Register repositories
        services.AddSingleton<IPatientRepository, CosmosDbPatientRepository>();

        return services;
    }
}