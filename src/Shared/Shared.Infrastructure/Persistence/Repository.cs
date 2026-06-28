using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Domain;

namespace Shared.Infrastructure.Persistence;

/// <summary>
/// Evaluates Specification<T> objects against IQueryable<T> to build EF Core queries.
/// Supports: Criteria filtering, Includes (eager loading), Ordering, Paging.
/// </summary>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Apply a specification to an IQueryable to build the final query.
    /// </summary>
    public static IQueryable<TEntity> GetQuery<TEntity>(
        IQueryable<TEntity> inputQuery,
        Specification<TEntity> specification) where TEntity : Entity
    {
        var query = inputQuery;

        // Apply criteria (WHERE clause)
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes (eager loading via expressions)
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply includes (eager loading via string navigation paths — for nested includes)
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging (SKIP + TAKE)
        if (specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
                query = query.Skip(specification.Skip.Value);
            if (specification.Take.HasValue)
                query = query.Take(specification.Take.Value);
        }

        return query;
    }
}

/// <summary>
/// Generic repository implementation using Entity Framework Core.
/// Implements IRepository<TEntity> with full CRUD, specification pattern, and soft delete.
/// </summary>
/// <typeparam name="TEntity">Entity type that extends the base Entity class</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    protected readonly DbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(DbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetBySpecificationAsync(
        Specification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator.GetQuery(DbSet.AsQueryable(), specification)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        Specification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }
        return await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Specification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator.GetQuery(DbSet.AsQueryable(), specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbContext.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Soft delete — calls Entity.Delete() which sets IsDeleted = true
        entity.Delete();
        DbContext.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.Delete();
            DbContext.Entry(entity).State = EntityState.Modified;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
