namespace Shared.Domain;

/// <summary>
/// Base class for domain events
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the domain event
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
    /// Gets or sets the causation ID (ID of the command that caused this event)
    /// </summary>
    public Guid? CausationId { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about the event
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];
}
