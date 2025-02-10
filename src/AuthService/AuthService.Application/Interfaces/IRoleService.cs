using AuthService.Application.DTOs;
using SharedLibrary.Models;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Service interface for managing roles and role assignments
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Creates a new role in the system
    /// </summary>
    /// <param name="request">The role creation request containing the role name</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<IResult> CreateRoleAsync(CreateRoleRequest request);

    /// <summary>
    /// Assigns a role to a specific user
    /// </summary>
    /// <param name="request">The role assignment request containing user ID and role name</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<IResult> AssignRoleAsync(AssignRoleRequest request);

    /// <summary>
    /// Updates the maximum number of allowed SuperAdmin users
    /// </summary>
    /// <param name="newLimit">The new maximum limit for SuperAdmin users</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<IResult> UpdateSuperAdminLimitAsync(int newLimit);

    /// <summary>
    /// Retrieves a list of all available roles in the system with their details
    /// </summary>
    /// <returns>A Result containing the list of roles with their details</returns>
    Task<IResult<ListOfRolesResponse>> ListRolesAsync();

    /// <summary>
    /// Retrieves all roles assigned to a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A Result containing the list of role names assigned to the user</returns>
    Task<IResult<IEnumerable<string>>> GetUserRolesAsync(string userId);
}