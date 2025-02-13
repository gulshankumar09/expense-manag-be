using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Constants;

namespace AuthService.API.Controllers;

/// <summary>
/// Controller for handling authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthController
    /// </summary>
    /// <param name="authService">The authentication service</param>
    /// <param name="googleAuthService">The Google authentication service</param>
    /// <param name="logger">The logger instance</param>
    public AuthController(
        IAuthService authService,
        IGoogleAuthService googleAuthService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _googleAuthService = googleAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user using email and password
    /// </summary>
    /// <param name="request">The login credentials</param>
    /// <returns>Authentication response with tokens if successful</returns>
    /// <response code="200">Returns authentication tokens when login is successful</response>
    /// <response code="400">Returns error message when login fails</response>
    /// <response code="401">Returns when credentials are invalid</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                Error { Code: ErrorConstants.Codes.UnauthorizedCode } => Unauthorized(result),
                _ => BadRequest(result)
            };
        }

        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user using Google OAuth
    /// </summary>
    /// <param name="request">The Google authentication request containing the ID token</param>
    /// <returns>Authentication response with tokens if successful</returns>
    /// <response code="200">Returns authentication tokens when Google login is successful</response>
    /// <response code="400">Returns error message when Google login fails</response>
    /// <response code="401">Returns when Google token is invalid</response>
    [HttpPost("google-login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<AuthResponse>>> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
        if (googleUser == null)
            return Unauthorized(Result.Failure(Error.Unauthorized()));

        var result = await _authService.GoogleLoginAsync(
            googleUser.Email,
            googleUser.GoogleId,
            googleUser.FirstName,
            googleUser.LastName);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                Error { Code: ErrorConstants.Codes.UnauthorizedCode } => Unauthorized(result),
                _ => BadRequest(result)
            };
        }

        return Ok(result);
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">The registration details</param>
    /// <returns>Authentication response with tokens if registration is successful</returns>
    /// <response code="200">Returns authentication tokens when registration is successful</response>
    /// <response code="400">Returns error message when registration fails</response>
    /// <response code="409">Returns when email already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                Error { Code: ErrorConstants.Codes.ConflictCode } => Conflict(result),
                _ => BadRequest(result)
            };
        }

        foreach (var header in result.Headers)
        {
            Response.Headers.Append(header.Key, header.Value);
        }

        return Ok(result);
    }

    /// <summary>
    /// Verifies a user's email address
    /// </summary>
    /// <param name="token">The verification token</param>
    /// <returns>Success message if email is verified</returns>
    /// <response code="200">Returns success message when email is verified</response>
    /// <response code="400">Returns error message when verification fails</response>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> VerifyEmail([FromBody] string token)
    {
        var result = await _authService.VerifyEmailAsync(token);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Initiates the password reset process
    /// </summary>
    /// <param name="request">The email address for password reset</param>
    /// <returns>Success message if reset email is sent</returns>
    /// <response code="200">Returns success message when reset email is sent</response>
    /// <response code="400">Returns error message when request fails</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Resets a user's password using a reset token
    /// </summary>
    /// <param name="request">The password reset details</param>
    /// <returns>Success message if password is reset</returns>
    /// <response code="200">Returns success message when password is reset</response>
    /// <response code="400">Returns error message when reset fails</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Refreshes the authentication tokens
    /// </summary>
    /// <param name="request">The refresh token request</param>
    /// <returns>New authentication tokens if refresh is successful</returns>
    /// <response code="200">Returns new authentication tokens</response>
    /// <response code="400">Returns error message when refresh fails</response>
    /// <response code="401">Returns when refresh token is invalid</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                Error { Code: ErrorConstants.Codes.UnauthorizedCode } => Unauthorized(result),
                _ => BadRequest(result)
            };
        }

        return Ok(result);
    }
}