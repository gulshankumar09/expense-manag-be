using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using SharedLibrary.Models;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Service interface for managing user-related operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the user</param>
    /// <returns>A Result containing the user if found, or an error message if not found</returns>
    Task<Result<User>> GetUserByIdAsync(Guid id);

    /// <summary>
    /// Retrieves a user by their email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>A Result containing the user if found, or an error message if not found</returns>
    Task<Result<User>> GetUserByEmailAsync(string email);

    /// <summary>
    /// Registers a new user in the system
    /// </summary>
    /// <param name="request">The registration details including email, password, and personal information</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<Result<string>> RegisterUserAsync(RegisterRequest request);

    /// <summary>
    /// Updates an existing user's profile information
    /// </summary>
    /// <param name="userId">The ID of the user to update</param>
    /// <param name="request">The updated user information</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<Result<string>> UpdateUserAsync(string userId, UpdateUserRequest request);

    /// <summary>
    /// Performs a soft delete of a user
    /// </summary>
    /// <param name="userId">The ID of the user to delete</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<Result<string>> DeleteUserAsync(string userId);

    /// <summary>
    /// Updates the roles assigned to a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="roles">The collection of role names to assign to the user</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<Result<string>> UpdateUserRolesAsync(string userId, IEnumerable<string> roles);
}