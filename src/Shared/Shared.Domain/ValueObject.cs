namespace Shared.Domain;

/// <summary>
/// Base class for value objects (immutable objects with no identity)
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Get the atomic values of the value object
    /// </summary>
    protected abstract IEnumerable<object?> GetAtomicValues();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var valueObject = (ValueObject)obj;

        return GetAtomicValues().SequenceEqual(valueObject.GetAtomicValues());
    }

    public bool Equals(ValueObject? other)
    {
        return Equals(other as object);
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(default(HashCode), (hashCode, value) =>
            {
                hashCode.Add(value);
                return hashCode;
            })
            .ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (Equals(left, null) ^ Equals(right, null))
            return false;

        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
