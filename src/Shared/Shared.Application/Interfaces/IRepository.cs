using Shared.Domain;

namespace Shared.Application.Interfaces;

/// <summary>
/// Generic repository interface using the Repository pattern
/// </summary>
public interface IRepository<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entities by specification
    /// </summary>
    Task<IEnumerable<TEntity>> GetBySpecificationAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count by specification
    /// </summary>
    Task<int> CountAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add entity
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity (hard delete)
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete multiple entities
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if entity exists by ID
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic repository for aggregate roots
/// </summary>
public interface IRepository<TEntity, in TId> where TEntity : AggregateRoot
{
    /// <summary>
    /// Get aggregate root by ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add aggregate root
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update aggregate root
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete aggregate root
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
