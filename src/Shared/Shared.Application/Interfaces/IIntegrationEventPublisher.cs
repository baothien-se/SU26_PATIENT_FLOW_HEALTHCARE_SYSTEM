namespace Shared.Application.Interfaces;

/// <summary>
/// Publisher for integration events (RabbitMQ)
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publish an integration event
    /// </summary>
    Task PublishAsync<TEvent>(TEvent domainEvent, string? routingKey = null, CancellationToken cancellationToken = default) where TEvent : class;

    /// <summary>
    /// Publish multiple integration events
    /// </summary>
    Task PublishAsync<TEvent>(IEnumerable<TEvent> domainEvents, string? routingKey = null, CancellationToken cancellationToken = default) where TEvent : class;
}

/// <summary>
/// Subscriber for integration events
/// </summary>
public interface IIntegrationEventHandler<in TEvent> where TEvent : class
{
    /// <summary>
    /// Handle an integration event
    /// </summary>
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
