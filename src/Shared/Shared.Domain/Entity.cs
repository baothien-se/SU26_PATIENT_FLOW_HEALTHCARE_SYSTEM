namespace Shared.Domain;

/// <summary>
/// Base entity class for all domain entities
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Gets the entity's unique identifier
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Gets the creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the last update timestamp (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Gets the user ID who created this entity
    /// </summary>
    public int? CreatedBy { get; protected set; }

    /// <summary>
    /// Gets the user ID who last updated this entity
    /// </summary>
    public int? UpdatedBy { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether this entity is marked as deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Gets the timestamp when this entity was soft deleted
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

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
    /// Update the last modified timestamp
    /// </summary>
    protected virtual void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity entity)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (GetType() != obj.GetType())
            return false;

        return Id == entity.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (Equals(left, null) ^ Equals(right, null))
            return false;

        return Equals(left, right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
