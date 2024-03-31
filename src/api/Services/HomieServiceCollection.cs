// (c) 2024 @Maxylan
namespace Microsoft.Extensions.DependencyInjection;

using Homie.Api.v1.Handlers;
using Homie.Database;
using Homie.Database.Models;

public static class HomieServiceCollection
{
    /// <summary>
    /// Add the Homie services to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddHomieServices(this IServiceCollection services)
    {
        // All-important HttpContextAccessor
        services.AddScoped<HttpContextAccessor>();

        // Singletons
        services.AddSingleton<HomieDB>();

        // Scoped
        services.AddScoped<PlatformsHandler>();
        services.AddScoped<UsersHandler>();

        return services;
    }
}