using Shared.Domain;

namespace Shared.Application.Interfaces;

/// <summary>
/// Publisher for domain events
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publish a domain event
    /// </summary>
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish multiple domain events
    /// </summary>
    Task PublishAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

/// <summary>
/// Subscriber for domain events
/// </summary>
public interface IDomainEventHandler<TEvent> where TEvent : DomainEvent
{
    /// <summary>
    /// Handle a domain event
    /// </summary>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
