namespace Shared.Application.Exceptions;

/// <summary>
/// Domain exception for business logic violations
/// </summary>
public class DomainException : Exception
{
    public DomainException(string? message) : base(message) { }

    public DomainException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception for when a resource is not found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string resourceName, int id)
        : base($"{resourceName} with ID {id} was not found.") { }

    public NotFoundException(string? message) : base(message) { }

    public NotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception for validation failures
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string? message = "Validation failed.")
        : base(message)
    {
        Errors = [];
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }
}

/// <summary>
/// Exception for business logic conflicts
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string? message) : base(message) { }

    public ConflictException(string? message, Exception? innerException) : base(message, innerException) { }
}
