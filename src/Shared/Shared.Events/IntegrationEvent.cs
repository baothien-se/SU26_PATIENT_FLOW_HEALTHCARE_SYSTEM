namespace Shared.Events;

/// <summary>
/// Base class for integration events (cross-service events)
/// </summary>
public abstract class IntegrationEvent
{
    /// <summary>
    /// Gets the unique identifier of the integration event
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when the event occurred (UTC)
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the correlation ID for tracking related events
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the source service that published this event
    /// </summary>
    public string? SourceService { get; set; }

    /// <summary>
    /// Gets additional metadata about the event
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets the event version for versioning support
    /// </summary>
    public int Version { get; set; } = 1;
}
