using Shared.Domain;

namespace Shared.Application.Interfaces;

/// <summary>
/// Generic repository interface using the Repository pattern.
/// Provides CRUD operations and specification-based queries for domain entities.
/// </summary>
public interface IRepository<TEntity> where TEntity : Entity
{
    /// <summary>
    /// Get entity by Guid ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities (use with caution — prefer specification-based queries for large datasets)
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entities matching a specification (Specification Pattern for encapsulated query logic)
    /// </summary>
    Task<IEnumerable<TEntity>> GetBySpecificationAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of entities matching a specification
    /// </summary>
    Task<int> CountAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new entity to the repository
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities in bulk
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing entity
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete an entity (sets IsDeleted = true)
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete multiple entities
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an entity exists by Guid ID
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the first entity matching a specification, or null
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
}
