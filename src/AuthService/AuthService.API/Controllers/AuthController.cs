using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using AuthService.Application.Interfaces;
using AuthService.Application.DTOs;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IOtpService _otpService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        IOtpService otpService,
        IGoogleAuthService googleAuthService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _otpService = otpService;
        _googleAuthService = googleAuthService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<Result<AuthResponse>>> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
        if (googleUser == null)
            return BadRequest(Result<AuthResponse>.Failure("Invalid Google token"));

        var result = await _userService.GoogleLoginAsync(
            googleUser.Email,
            googleUser.GoogleId,
            googleUser.FirstName,
            googleUser.LastName);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

}