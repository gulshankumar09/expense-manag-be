using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Application.Services;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<string>>> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user == null || !VerifyPassword(request.Password, user.Data.PasswordHash)) // Implement password verification
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
    public async Task<ActionResult<Result<string>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterUserAsync(
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName);

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

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FirstName, string LastName); 