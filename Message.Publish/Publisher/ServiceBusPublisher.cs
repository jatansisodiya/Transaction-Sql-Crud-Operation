namespace Message.Publish.Publisher;

using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using CommonLogger;
using Message.Publish.Interface;
using Message.Publish.Model;

/// <summary>
/// Service Bus implementation of <see cref="IServiceBusPublisher"/> for dynamic queue/topic publishing.
/// </summary>
public class ServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly IAILogger _logger;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders;

    public ServiceBusPublisher(ServiceBusClient client, IAILogger logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _senders = new ConcurrentDictionary<string, ServiceBusSender>();
    }

    /// <inheritdoc />
    public Task SendToQueueAsync(string queueName, ServiceBusEventMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(message);

        return SendMessageInternalAsync(queueName, message, "Queue", cancellationToken);
    }

    /// <inheritdoc />
    public Task SendToTopicAsync(string topicName, ServiceBusEventMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);
        ArgumentNullException.ThrowIfNull(message);

        return SendMessageInternalAsync(topicName, message, "Topic", cancellationToken);
    }

    private async Task SendMessageInternalAsync(string destinationName, ServiceBusEventMessage message, string destinationType, CancellationToken cancellationToken)
    {
        try
        {
            var sender = GetOrCreateSender(destinationName);
            var sbMessage = CreateServiceBusMessage(message);

            _logger.LogInformation($"Sending message {message.Id} to {destinationType} '{destinationName}'");
            
            await sender.SendMessageAsync(sbMessage, cancellationToken);

            _logger.LogInformation($"Successfully published message {message.Id} to {destinationType} '{destinationName}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to publish message {message?.Id} to {destinationType} '{destinationName}'");
            throw;
        }
    }

    private static ServiceBusMessage CreateServiceBusMessage(ServiceBusEventMessage message)
    {
        var jsonPayload = JsonSerializer.Serialize(message, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
            WriteIndented = true 
        });

        var sbMessage = new ServiceBusMessage(jsonPayload)
        {
            MessageId = message.Id,
            ContentType = "application/json",
            Subject = message.Type
        };

        if (message.ScheduledEnqueueMinute > 0)
        {
            sbMessage.ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddMinutes(message.ScheduledEnqueueMinute);
        }

        return sbMessage;
    }

    private ServiceBusSender GetOrCreateSender(string entityPath)
    {
        return _senders.GetOrAdd(entityPath, path => _client.CreateSender(path));
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }

        _senders.Clear();
        GC.SuppressFinalize(this);
    }
}
