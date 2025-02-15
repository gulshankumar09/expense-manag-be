using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Constants;
using Microsoft.AspNetCore.Authorization;
using ApiResults = SharedLibrary.Models;
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
    /// <param name="cancellationToken"></param>
    /// <returns>Authentication response with tokens if successful</returns>
    /// <response code="200">Returns authentication tokens when login is successful</response>
    /// <response code="400">Returns error message when login fails</response>
    /// <response code="401">Returns when credentials are invalid</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

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
    /// <param name="cancellationToken"></param>
    /// <returns>Authentication response with tokens if successful</returns>
    /// <response code="200">Returns authentication tokens when Google login is successful</response>
    /// <response code="400">Returns error message when Google login fails</response>
    /// <response code="401">Returns when Google token is invalid</response>
    [HttpPost("google-login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<AuthResponse>>> GoogleLogin([FromBody] GoogleAuthRequest request, CancellationToken cancellationToken)
    {
        var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken, cancellationToken);
        if (googleUser == null)
            return Unauthorized(Result.Failure(Error.Unauthorized()));

        var result = await _authService.GoogleLoginAsync(
            googleUser.Email,
            googleUser.GoogleId,
            googleUser.FirstName,
            googleUser.LastName,
            cancellationToken);

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
    /// Refreshes the authentication tokens
    /// </summary>
    /// <param name="request">The refresh token request</param>
    /// <param name="cancellationToken"></param>
    /// <returns>New authentication tokens if refresh is successful</returns>
    /// <response code="200">Returns new authentication tokens</response>
    /// <response code="400">Returns error message when refresh fails</response>
    /// <response code="401">Returns when refresh token is invalid</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);

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
    /// Logs out the current user
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResults.IResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResults.IResult>> Logout(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return BadRequest(ApiResults.Result.Failure(Error.BadRequest("User ID not found in token")));

        var result = await _authService.LogoutAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}