using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SharedLibrary.Models;
using AuthService.Application.Interfaces;
using AuthService.Application.DTOs;

namespace AuthService.API.Controllers;

/// <summary>
/// Controller for handling user account and authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IUserService userService,
        IAuthService authService,
        ILogger<AccountController> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">The registration details</param>
    /// <returns>A success message if registration is successful, or error details if registration fails</returns>
    /// <response code="200">Returns success message when registration is successful</response>
    /// <response code="400">Returns error message when registration fails</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<string>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterUserAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Verifies user's email using OTP
    /// </summary>
    /// <param name="request">The email and OTP verification details</param>
    /// <returns>Authentication response with tokens if verification is successful</returns>
    /// <response code="200">Returns authentication tokens when verification is successful</response>
    /// <response code="400">Returns error message when verification fails</response>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<AuthResponse>>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _authService.VerifyOtpAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Initiates the password reset process
    /// </summary>
    /// <param name="request">The email address for password reset</param>
    /// <returns>A success message indicating that reset instructions have been sent</returns>
    /// <response code="200">Returns success message when reset email is sent</response>
    /// <response code="400">Returns error message when the request fails</response>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<Result<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Resets user's password using reset token
    /// </summary>
    /// <param name="request">The password reset details including token</param>
    /// <returns>A success message if password is reset successfully</returns>
    /// <response code="200">Returns success message when password is reset</response>
    /// <response code="400">Returns error message when reset fails</response>
    [HttpPost("reset-password")]
    public async Task<ActionResult<Result<string>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Changes the authenticated user's password
    /// </summary>
    /// <param name="request">The current and new password details</param>
    /// <returns>A success message if password is changed successfully</returns>
    /// <response code="200">Returns success message when password is changed</response>
    /// <response code="400">Returns error message when change fails</response>
    /// <response code="401">Returns when user is not authenticated</response>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<Result<string>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return BadRequest(Result<string>.Failure("User not found"));

        var result = await _authService.ChangePasswordAsync(userId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Updates the authenticated user's profile information
    /// </summary>
    /// <param name="request">The updated user information</param>
    /// <returns>A success message if profile is updated successfully</returns>
    /// <response code="200">Returns success message when profile is updated</response>
    /// <response code="400">Returns error message when update fails</response>
    /// <response code="401">Returns when user is not authenticated</response>
    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<Result<string>>> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return BadRequest(Result<string>.Failure("User not found"));

        var result = await _userService.UpdateUserAsync(userId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Refreshes the authentication tokens using a refresh token
    /// </summary>
    /// <param name="refreshToken">The current refresh token</param>
    /// <returns>New authentication tokens if refresh is successful</returns>
    /// <response code="200">Returns new authentication tokens</response>
    /// <response code="400">Returns error message when refresh fails</response>
    /// <response code="401">Returns when user is not authenticated</response>
    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<Result<AuthResponse>>> RefreshToken([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Revokes the authenticated user's refresh token
    /// </summary>
    /// <returns>A success message if token is revoked successfully</returns>
    /// <response code="200">Returns success message when token is revoked</response>
    /// <response code="400">Returns error message when revocation fails</response>
    /// <response code="401">Returns when user is not authenticated</response>
    [Authorize]
    [HttpPost("revoke-token")]
    public async Task<ActionResult<Result<string>>> RevokeToken()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return BadRequest(Result<string>.Failure("User not found"));

        var result = await _authService.RevokeTokenAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}