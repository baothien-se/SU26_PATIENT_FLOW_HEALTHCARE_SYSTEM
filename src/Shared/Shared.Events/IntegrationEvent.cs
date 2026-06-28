namespace Shared.Events;

/// <summary>
/// Base class for integration events (cross-service communication via RabbitMQ).
/// Integration events are published by one microservice and consumed by others.
/// They are different from domain events which are internal to a single service.
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
    /// Gets the correlation ID for tracking related events across services
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
    /// Gets the event version for schema evolution support
    /// </summary>
    public int Version { get; set; } = 1;
}
