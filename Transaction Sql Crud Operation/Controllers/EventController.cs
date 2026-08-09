namespace Transaction_Sql_Crud_Operation.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Message.Publish.Interface;
using Message.Publish.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// API Controller for publishing events to Azure Service Bus.
/// Delegates messaging operations to <see cref="IServiceBusPublisher"/>.
/// </summary>
[ApiController]
[Route("api/v1/events")]
[Produces("application/json")]
public class EventController : ControllerBase
{
    private readonly IServiceBusPublisher? _publisher;
    private readonly ILogger<EventController> _logger;

    public EventController(ILogger<EventController> logger, IServiceBusPublisher? publisher = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publisher = publisher;
    }

    /// <summary>
    /// Publishes an event message to a specified queue.
    /// </summary>
    [HttpPost("queues/{queueName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PublishQueueEvent(
        [FromRoute] string queueName,
        [FromBody] ServiceBusEventMessage message)
    {
        if (string.IsNullOrWhiteSpace(queueName) || message == null)
        {
            return BadRequest(new { message = "Invalid queue name or payload." });
        }

        if (_publisher == null)
        {
            return BadRequest(new { message = "Message publisher service is not configured." });
        }

        _logger.LogInformation("Publishing event {MessageId} to queue '{QueueName}'", message.Id, queueName);
        await _publisher.SendToQueueAsync(queueName, message);
        
        return Ok(new { message = "Message dispatched to queue successfully.", messageId = message.Id });
    }

    /// <summary>
    /// Publishes an event message to a specified topic.
    /// </summary>
    [HttpPost("topics/{topicName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PublishTopicEvent(
        [FromRoute] string topicName,
        [FromBody] ServiceBusEventMessage message,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(topicName) || message == null)
        {
            return BadRequest(new { message = "Invalid topic name or payload." });
        }

        if (_publisher == null)
        {
            return BadRequest(new { message = "Message publisher service is not configured." });
        }

        _logger.LogInformation("Publishing event {MessageId} to topic '{TopicName}'", message.Id, topicName);
        await _publisher.SendToTopicAsync(topicName, message, cancellationToken);

        return Ok(new { message = "Message dispatched to topic successfully.", messageId = message.Id });
    }
}
