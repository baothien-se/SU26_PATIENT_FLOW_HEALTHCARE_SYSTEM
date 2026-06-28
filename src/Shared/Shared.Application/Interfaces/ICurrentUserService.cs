namespace Shared.Application.Interfaces;

/// <summary>
/// Current user context extracted from JWT token claims.
/// Provides information about the authenticated user making the current request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the current user's ID (from JWT sub claim, string for ASP.NET Identity compatibility)
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Get the current user's name
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Get the current user's email
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Get the current user's department ID (if applicable)
    /// </summary>
    Guid? DepartmentId { get; }

    /// <summary>
    /// Check if the current user is in a specific role
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Get all roles for the current user
    /// </summary>
    IEnumerable<string> GetRoles();

    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}
