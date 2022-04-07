using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Neo4j.Driver;

namespace Neo4j.OGM;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Try and register the Neo4j OGM services in the DI container.
    /// </summary>
    public static IServiceCollection AddNeo4jOGMFactory(
        this IServiceCollection serviceCollection,
        string connectionString,
        IAuthToken authToken,
        params Assembly[] assemblies
    )
    {
        serviceCollection.TryAddSingleton(
            new SessionFactory(connectionString, authToken, assemblies));
        return serviceCollection;
    }
}
