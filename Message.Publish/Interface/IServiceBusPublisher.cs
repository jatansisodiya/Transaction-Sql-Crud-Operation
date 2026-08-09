namespace Message.Publish.Interface;

using Message.Publish.Model;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Interface for publishing messages dynamically to Azure Service Bus queues and topics.
/// </summary>
public interface IServiceBusPublisher
{
    /// <summary>
    /// Sends an event message to a specified Azure Service Bus queue.
    /// </summary>
    /// <param name="queueName">Target queue name.</param>
    /// <param name="message">Event payload message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendToQueueAsync(string queueName, ServiceBusEventMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an event message to a specified Azure Service Bus topic.
    /// </summary>
    /// <param name="topicName">Target topic name.</param>
    /// <param name="message">Event payload message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendToTopicAsync(string topicName, ServiceBusEventMessage message, CancellationToken cancellationToken = default);
}
