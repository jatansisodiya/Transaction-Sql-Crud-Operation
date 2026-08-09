namespace Message.Publish.Infrastructure;

using System;
using Azure.Messaging.ServiceBus;
using Message.Publish.Interface;
using Message.Publish.Publisher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up Service Bus publisher services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceBusPublisherExtensions
{
    /// <summary>
    /// Adds Azure Service Bus client and <see cref="IServiceBusPublisher"/> to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMessagePublisher(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("ServiceBusConnection") 
            ?? configuration["ServiceBus:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Service Bus connection string missing from configuration ('ConnectionStrings:ServiceBusConnection' or 'ServiceBus:ConnectionString').");
        }

        services.AddSingleton(_ => new ServiceBusClient(connectionString));
        services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();

        return services;
    }
}
