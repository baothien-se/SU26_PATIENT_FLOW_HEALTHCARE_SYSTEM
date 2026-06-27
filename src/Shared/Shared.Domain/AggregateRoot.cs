namespace Shared.Domain;

/// <summary>
/// Base class for aggregate roots in Domain-Driven Design
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _uncommittedEvents = [];

    /// <summary>
    /// Gets the uncommitted domain events
    /// </summary>
    public IReadOnlyList<DomainEvent> GetUncommittedEvents()
    {
        return _uncommittedEvents.AsReadOnly();
    }

    /// <summary>
    /// Clear uncommitted events after they have been persisted
    /// </summary>
    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }

    /// <summary>
    /// Raise a domain event
    /// </summary>
    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _uncommittedEvents.Add(domainEvent);
    }
}
