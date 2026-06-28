using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Domain;

namespace Shared.Infrastructure.Persistence;

/// <summary>
/// Base DbContext for all microservice databases.
/// Provides:
/// - Automatic audit field population (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
/// - Soft delete handling (global query filter for IsDeleted)
/// - Domain event dispatching AFTER successful save (not before!)
/// - Integration with ICurrentUserService for audit trails
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly IDomainEventPublisher? _domainEventPublisher;

    protected BaseDbContext(DbContextOptions options) : base(options) { }

    protected BaseDbContext(
        DbContextOptions options,
        ICurrentUserService? currentUserService,
        IDomainEventPublisher? domainEventPublisher = null) : base(options)
    {
        _currentUserService = currentUserService;
        _domainEventPublisher = domainEventPublisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyGlobalFilters(modelBuilder);
    }

    /// <summary>
    /// Override SaveChangesAsync to:
    /// 1. Populate audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
    /// 2. Handle soft deletes
    /// 3. Dispatch domain events AFTER successful persistence
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Step 1: Process audit fields and soft deletes before saving
        ProcessAuditableEntities();

        // Step 2: Collect domain events before save (they'll be cleared after dispatch)
        var domainEvents = CollectDomainEvents();

        // Step 3: Persist changes to database
        var result = await base.SaveChangesAsync(cancellationToken);

        // Step 4: Dispatch domain events AFTER successful save (ensures consistency)
        if (domainEvents.Count > 0 && _domainEventPublisher is not null)
        {
            await _domainEventPublisher.PublishAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Apply global query filter to exclude soft-deleted entities.
    /// All entities inheriting from Entity will automatically filter out IsDeleted = true.
    /// </summary>
    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(BaseDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, [modelBuilder]);
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : Entity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// Process all tracked entities to set audit fields and handle soft deletes.
    /// </summary>
    private void ProcessAuditableEntities()
    {
        var entries = ChangeTracker.Entries<Entity>();
        var currentUserId = _currentUserService?.UserId;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreatedAudit(currentUserId);
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdatedAudit(currentUserId);
                    // Prevent modification of CreatedAt and CreatedBy
                    entry.Property(nameof(Entity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(Entity.CreatedBy)).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // Convert hard delete to soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.Delete();
                    entry.Entity.SetUpdatedAudit(currentUserId);
                    break;
            }
        }
    }

    /// <summary>
    /// Collect domain events from all aggregate roots being tracked,
    /// then clear them from the aggregates.
    /// </summary>
    private List<DomainEvent> CollectDomainEvents()
    {
        var aggregateRoots = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.GetUncommittedEvents().Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(ar => ar.GetUncommittedEvents())
            .ToList();

        aggregateRoots.ForEach(ar => ar.ClearUncommittedEvents());

        return domainEvents;
    }
}
