namespace Shared.Domain;

/// <summary>
/// Base entity class for all domain entities.
/// Uses Guid as the identifier type for microservice-safe unique IDs.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>
    /// Gets the entity's unique identifier (Guid for cross-service uniqueness)
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Gets the creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the last update timestamp (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Gets the user ID who created this entity (string for ASP.NET Identity compatibility)
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// Gets the user ID who last updated this entity
    /// </summary>
    public string? UpdatedBy { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether this entity is marked as deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Gets the timestamp when this entity was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// Protected parameterless constructor for EF Core
    /// </summary>
    protected Entity() { }

    /// <summary>
    /// Protected constructor with ID for factory methods
    /// </summary>
    protected Entity(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Soft delete the entity
    /// </summary>
    public virtual void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore a soft deleted entity
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }

    /// <summary>
    /// Set audit info for creation
    /// </summary>
    public void SetCreatedAudit(string? userId)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
    }

    /// <summary>
    /// Set audit info for update
    /// </summary>
    public void SetUpdatedAudit(string? userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    /// <summary>
    /// Update the last modified timestamp
    /// </summary>
    protected virtual void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (Id == Guid.Empty || other.Id == Guid.Empty) return false;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
