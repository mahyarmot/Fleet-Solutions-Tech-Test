using Microsoft.Extensions.DependencyInjection;
using NhsTechTest.Domain.Repositories;
using NhsTechTest.Infrastructure.Persistence;

namespace NhsTechTest.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPatientRepository, InMemoryPatientRepository>();

        return services;
    }
}
