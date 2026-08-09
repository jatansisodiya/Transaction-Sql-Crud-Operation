namespace Real_Time_MCP.FunctionApp.ServiceBusFunctions;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommonLogger;
using Message.Publish.Model;
using Microsoft.Azure.Functions.Worker;
using Real_Time_MCP.FunctionApp.Interface;

/// <summary>
/// Azure Function trigger class for processing Service Bus queue messages.
/// Contains no inline business logic; delegates execution to <see cref="IQualificationMessageProcessor"/>.
/// </summary>
public class ServiceBusQueTriggerFunctions
{
    private readonly IQualificationMessageProcessor _processor;
    private readonly IAILogger _logger;

    public ServiceBusQueTriggerFunctions(IQualificationMessageProcessor processor, IAILogger logger)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Function trigger executed when a message arrives on the qualification queue.
    /// </summary>
    [Function("ProcessQualification")]
    public async Task RunAsync(
        [ServiceBusTrigger("qualification", Connection = "ServiceBusConnectionString")] string messageText,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service Bus trigger function received queue message.");

        var message = JsonSerializer.Deserialize<ServiceBusEventMessage>(messageText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (message == null)
        {
            _logger.LogWarning("Deserialized ServiceBusEventMessage is null. Message payload: {Payload}", messageText);
            return;
        }

        await _processor.ProcessMessageAsync(message, cancellationToken);
    }
}
