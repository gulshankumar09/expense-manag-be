using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
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
    public async Task<ActionResult<Result<string>>> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user == null || !VerifyPassword(request.Password, user.Data.PasswordHash.Hash)) // Implement password verification
        {
            return Unauthorized(Result<string>.Failure("Invalid credentials"));
        }

        var token = GenerateJwtToken(user.Data);
        return Ok(Result<string>.Success(token));
    }
    
    private bool VerifyPassword(string password, string passwordHash)
    {
        // Implement your password verification logic here
        // For example, using BCrypt or any hashing algorithm
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<string>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _userService.RegisterUserAsync(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PhoneNumber);

            if (!result.IsSuccess)
                return BadRequest(result);

            // Generate and send OTP
            var otp = _otpService.GenerateOtp();
            await _otpService.SendOtpAsync(request.Email, otp);

            return Ok(Result<string>.Success("Registration successful. Please verify your email."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return StatusCode(500, Result<string>.Failure("An error occurred during registration."));
        }
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<Result<AuthResponse>>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _userService.VerifyOtpAsync(request.Email, request.Otp);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
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

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKeyHere")); // Use the same key as in appsettings.json
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "YourIssuer",
            audience: "YourAudience",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}