using AuthService.Application.DTOs; 
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using ApiResults = SharedLibrary.Models;
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
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResults.IResult>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _authService.RegisterAsync(request);

        foreach (var header in result.Headers)
        {
            Response.Headers.Append(header.Key, header.Value);
        }

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Authenticates a user and generates access tokens
    /// </summary>
    /// <param name="request">The login credentials</param>
    /// <returns>Authentication response with tokens if verification is successful</returns>
    /// <response code="200">Returns authentication tokens when verification is successful</response>
    /// <response code="400">Returns error message when verification fails</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(IResult<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IResult<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Verifies a user's email address
    /// </summary>
    /// <param name="token">The verification token</param>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResults.IResult>> VerifyEmail([FromBody] string token)
    {
        var result = await _authService.VerifyEmailAsync(token);
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
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResults.IResult>> ForgotPassword([FromBody] ForgotPasswordRequest request)
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
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResults.IResult>> ResetPassword([FromBody] ResetPasswordRequest request)
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
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResults.IResult>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _authService.ChangePasswordAsync(request);
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
    [ProducesResponseType(typeof(ApiResults.IResult<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResults.IResult<UserResponse>>> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return BadRequest(ApiResults.Result.Failure(Error.BadRequest("User ID not found in token")));

        var result = await _userService.UpdateUserAsync(userId, request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Refreshes the authentication tokens using a refresh token
    /// </summary>
    /// <param name="request">The refresh token request</param>
    /// <returns>New authentication tokens if refresh is successful</returns>
    /// <response code="200">Returns new authentication tokens</response>
    /// <response code="400">Returns error message when refresh fails</response>
    /// <response code="401">Returns when user is not authenticated</response>
    [Authorize]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResults.IResult<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResults.IResult<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Logs out the current user
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResults.IResult>> Logout()
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return BadRequest(ApiResults.Result.Failure(Error.BadRequest("User ID not found in token")));

        var result = await _authService.LogoutAsync(userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}