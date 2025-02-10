using AuthService.Application.DTOs;
using SharedLibrary.Models;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Service interface for authentication and authorization operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user in the system
    /// </summary>
    Task<IResult<AuthResponse>> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user and generates access tokens
    /// </summary>
    Task<IResult<AuthResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// Refreshes the access token using a valid refresh token
    /// </summary>
    Task<IResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    Task<IResult> LogoutAsync(string userId);

    /// <summary>
    /// Verifies a user's email address using a verification token
    /// </summary>
    Task<IResult> VerifyEmailAsync(string token);

    /// <summary>
    /// Initiates the password reset process by sending a reset link
    /// </summary>
    Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>
    /// Resets a user's password using a valid reset token
    /// </summary>
    Task<IResult> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Changes a user's password after verifying their current password
    /// </summary>
    Task<IResult> ChangePasswordAsync(ChangePasswordRequest request);
}