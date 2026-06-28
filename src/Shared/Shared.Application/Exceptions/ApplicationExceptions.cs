namespace Shared.Application.Exceptions;

/// <summary>
/// Domain exception for business logic violations.
/// Thrown when a business rule is broken within the domain layer.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string? message) : base(message) { }

    public DomainException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception for when a resource is not found.
/// Results in HTTP 404 response.
/// </summary>
public class NotFoundException : Exception
{
    public string ResourceName { get; }
    public object Key { get; }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with key '{key}' was not found.")
    {
        ResourceName = resourceName;
        Key = key;
    }

    public NotFoundException(string? message) : base(message)
    {
        ResourceName = string.Empty;
        Key = string.Empty;
    }

    public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
        ResourceName = string.Empty;
        Key = string.Empty;
    }
}

/// <summary>
/// Exception for validation failures.
/// Contains structured validation errors grouped by property name.
/// Results in HTTP 400 response.
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
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

/// <summary>
/// Exception for business logic conflicts (e.g., duplicate resources).
/// Results in HTTP 409 response.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string? message) : base(message) { }

    public ConflictException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception for unauthorized access attempts.
/// Results in HTTP 403 response.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string? message = "You do not have permission to perform this action.")
        : base(message) { }
}
