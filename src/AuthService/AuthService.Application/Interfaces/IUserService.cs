using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using SharedLibrary.Models;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Service interface for managing user operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    Task<IResult<UserResponse>> GetUserByIdAsync(string userId);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    Task<IResult<UserResponse>> GetUserByEmailAsync(string email);

    /// <summary>
    /// Updates a user's profile information
    /// </summary>
    Task<IResult<UserResponse>> UpdateUserAsync(string userId, UpdateUserRequest request);

    /// <summary>
    /// Deactivates a user's account
    /// </summary>
    Task<IResult> DeactivateUserAsync(string userId);

    /// <summary>
    /// Reactivates a previously deactivated user's account
    /// </summary>
    Task<IResult> ReactivateUserAsync(string userId);

    /// <summary>
    /// Permanently deletes a user's account and all associated data
    /// </summary>
    Task<IResult> DeleteUserAsync(string userId);

    /// <summary>
    /// Lists all users with optional filtering and pagination
    /// </summary>
    Task<IResult<PaginatedResponse<UserResponse>>> ListUsersAsync(UserListRequest request);
}