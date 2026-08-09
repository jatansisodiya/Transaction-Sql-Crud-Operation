namespace Real_Time_MCP.FunctionApp.Repository;

using System;
using System.Threading;
using System.Threading.Tasks;
using CommonLogger;
using Message.Publish.Model;
using Real_Time_MCP.FunctionApp.Interface;

/// <summary>
/// Domain processor handling business logic for qualification Service Bus messages.
/// </summary>
public class QualificationMessageProcessor : IQualificationMessageProcessor
{
    private readonly IAILogger _aiLogger;

    public QualificationMessageProcessor(IAILogger aiLogger)
    {
        _aiLogger = aiLogger ?? throw new ArgumentNullException(nameof(aiLogger));
    }

    /// <inheritdoc />
    public Task ProcessMessageAsync(ServiceBusEventMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        _aiLogger.LogInformation($"Processing qualification event message {message.Id} of type '{message.Type}'");

        // Domain processing logic executed here
        
        _aiLogger.LogInformation("Successfully processed qualification event message {MessageId}", message.Id);

        return Task.CompletedTask;
    }
}
