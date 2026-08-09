namespace Real_Time_MCP.FunctionApp.Interface;

using System.Threading;
using System.Threading.Tasks;
using Message.Publish.Model;

/// <summary>
/// Service interface for processing incoming qualification messages from Azure Service Bus.
/// </summary>
public interface IQualificationMessageProcessor
{
    /// <summary>
    /// Processes a qualification event message.
    /// </summary>
    /// <param name="message">The deserialized event message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ProcessMessageAsync(ServiceBusEventMessage message, CancellationToken cancellationToken = default);
}
