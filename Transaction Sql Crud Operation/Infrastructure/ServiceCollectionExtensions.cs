using System.Reflection;
using Transaction.SQLConnection.Interfaces;
using Transaction.SQLConnection.Sql;

namespace Transaction_Sql_Crud_Operation.Infrastructure;

/// <summary>
/// Extension methods for registering Transaction.SQLConnection services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Transaction.SQLConnection services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionStringName">Name of the connection string in configuration (default: "DefaultConnection").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTransactionSqlConnection(
        this IServiceCollection services,
        string connectionStringName = "DefaultConnection")
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register connection factory as singleton
        services.AddSingleton<IConnectionFactoryAsync>(sp =>
        {
            var configuration = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ConnectionFactoryAsync>>();
            return new ConnectionFactoryAsync(configuration, logger, connectionStringName);
        });

        // Register executor as singleton (stateless)
        services.AddSingleton<IDbExecutorAsync, DbExecutorAsync>();

        // Register transactional repository as scoped (stateful per request)
        services.AddScoped<ITransactionalRepositoryAsync, TransactionalRepositoryAsync>();
        services.AddScoped<TransactionalRepositoryAsync>();

        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all repositories from the specified assembly.
    /// Repositories are classes ending with "Repository" that implement an interface.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for repositories. If null, uses the calling assembly.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Assembly assembly = Assembly.GetCallingAssembly();

        // Get all classes ending with "Repository" (excluding TransactionalRepositoryAsync which is registered separately)
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass
                && !t.IsAbstract
                && t.Name.EndsWith("Repository")
                && t != typeof(TransactionalRepositoryAsync));

        foreach (var repoType in repositoryTypes)
        {
            // Find the interface that matches "I{RepositoryName}" pattern or the first interface
            var interfaceType = repoType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{repoType.Name}")
                ?? repoType.GetInterfaces().FirstOrDefault();

            if (interfaceType == null)
                continue;

            services.AddScoped(interfaceType, repoType);
        }

        return services;
    }

    /// <summary>
    /// Adds a custom repository to the DI container.
    /// </summary>
    /// <typeparam name="TInterface">Repository interface type.</typeparam>
    /// <typeparam name="TImplementation">Repository implementation type.</typeparam>
    public static IServiceCollection AddRepository<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddScoped<TInterface, TImplementation>();
        return services;
    }
}