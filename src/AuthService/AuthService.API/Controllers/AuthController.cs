using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

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
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthController
    /// </summary>
    /// <param name="authService">The authentication service</param>
    /// <param name="googleAuthService">The Google authentication service</param>
    /// <param name="configuration">The application configuration</param>
    /// <param name="logger">The logger instance</param>
    public AuthController(
        IAuthService authService,
        IGoogleAuthService googleAuthService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _googleAuthService = googleAuthService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user using email and password
    /// </summary>
    /// <param name="request">The login credentials</param>
    /// <returns>Authentication response with tokens if successful</returns>
    /// <response code="200">Returns authentication tokens when login is successful</response>
    /// <response code="400">Returns error message when login fails</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }

    /// <summary>
    /// Authenticates a user using Google OAuth
    /// </summary>
    /// <param name="request">The Google authentication request containing the ID token</param>
    /// <returns>Authentication response with tokens if successful</returns>
    /// <response code="200">Returns authentication tokens when Google login is successful</response>
    /// <response code="400">Returns error message when Google login fails</response>
    [HttpPost("google-login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<AuthResponse>>> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
        if (googleUser == null)
            return BadRequest(Result<AuthResponse>.Failure("Invalid Google token"));

        var result = await _authService.GoogleLoginAsync(
            googleUser.Email,
            googleUser.GoogleId,
            googleUser.FirstName,
            googleUser.LastName);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}