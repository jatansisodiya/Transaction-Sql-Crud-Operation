namespace Message.Publish.Model;

using System;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a generic event message for Service Bus communication.
/// </summary>
public class ServiceBusEventMessage 
{
    /// <summary>
    /// Gets or sets the unique identifier for the event message.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the creation date and time of the event message.
    /// </summary>
    [Required]
    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the type of the event message.
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action associated with the event message.
    /// </summary>
    [Required]
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payload of the event message as an ExpandoObject.
    /// </summary>
    [Required]
    [JsonPropertyName("payload")]
    public ExpandoObject Payload { get; set; } = new ExpandoObject();

    /// <summary>
    /// Gets or sets the number of minutes to delay the scheduled enqueue time.
    /// </summary>
    [JsonPropertyName("scheduledEnqueueMinute")]
    public int ScheduledEnqueueMinute { get; set; }
}
