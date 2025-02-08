using AuthService.Application.DTOs;
using SharedLibrary.Models;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Service interface for handling authentication and authorization operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user using email and password
    /// </summary>
    /// <param name="request">The login credentials</param>
    /// <returns>A Result containing authentication response with tokens if successful</returns>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// Authenticates or creates a user using Google OAuth credentials
    /// </summary>
    /// <param name="email">The user's email from Google</param>
    /// <param name="googleId">The unique Google ID</param>
    /// <param name="firstName">The user's first name</param>
    /// <param name="lastName">The user's last name</param>
    /// <returns>A Result containing authentication response with tokens if successful</returns>
    Task<Result<AuthResponse>> GoogleLoginAsync(string email, string googleId, string firstName, string lastName);

    /// <summary>
    /// Verifies a user's email using OTP
    /// </summary>
    /// <param name="request">The email and OTP verification details</param>
    /// <returns>A Result containing authentication response with tokens if successful</returns>
    Task<Result<AuthResponse>> VerifyOtpAsync(VerifyOtpRequest request);

    /// <summary>
    /// Initiates the password reset process for a user
    /// </summary>
    /// <param name="request">The email address for password reset</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>
    /// Resets a user's password using the reset token
    /// </summary>
    /// <param name="request">The password reset details including token</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<Result<string>> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Generates new access and refresh tokens using a valid refresh token
    /// </summary>
    /// <param name="refreshToken">The current refresh token</param>
    /// <returns>A Result containing new authentication tokens if successful</returns>
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Invalidates a user's refresh token
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<Result<string>> RevokeTokenAsync(string userId);

    /// <summary>
    /// Changes a user's password
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="request">The current and new password details</param>
    /// <returns>A Result containing a success message or error details</returns>
    Task<Result<string>> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}