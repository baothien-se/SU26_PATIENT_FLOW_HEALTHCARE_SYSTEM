namespace Shared.Domain;

/// <summary>
/// Base class for aggregate roots in Domain-Driven Design.
/// Aggregate roots are the only entities that can be directly accessed from outside the aggregate.
/// They manage domain events and ensure aggregate consistency.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _uncommittedEvents = [];

    /// <summary>
    /// Protected parameterless constructor for EF Core
    /// </summary>
    protected AggregateRoot() : base() { }

    /// <summary>
    /// Protected constructor with ID for factory methods
    /// </summary>
    protected AggregateRoot(Guid id) : base(id) { }

    /// <summary>
    /// Gets the uncommitted domain events that have been raised but not yet dispatched
    /// </summary>
    public IReadOnlyList<DomainEvent> GetUncommittedEvents()
    {
        return _uncommittedEvents.AsReadOnly();
    }

    /// <summary>
    /// Clear uncommitted events after they have been dispatched/persisted
    /// </summary>
    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }

    /// <summary>
    /// Raise a domain event. Events are collected and dispatched after persistence (eventual consistency).
    /// </summary>
    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _uncommittedEvents.Add(domainEvent);
    }
}
