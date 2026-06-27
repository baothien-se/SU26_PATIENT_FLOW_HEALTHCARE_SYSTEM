namespace Shared.Application.Interfaces;

/// <summary>
/// Current user context
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the current user's ID
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Get the current user's name
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Get the current user's email
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Get the current user's department ID
    /// </summary>
    int? DepartmentId { get; }

    /// <summary>
    /// Check if the current user is in a specific role
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Get all roles for the current user
    /// </summary>
    IEnumerable<string> GetRoles();
}
