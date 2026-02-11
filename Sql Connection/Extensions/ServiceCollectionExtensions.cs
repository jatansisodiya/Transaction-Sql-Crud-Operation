using Microsoft.Extensions.DependencyInjection;
using Transaction.SQLConnection.Interfaces;
using Transaction.SQLConnection.Sql;

namespace Transaction.SQLConnection.Extensions;

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