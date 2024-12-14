using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<Result<string>>> Login([FromBody] LoginRequest request)
    {
        // Implementation will go here
        return Ok(Result<string>.Success("JWT Token will be returned here"));
    }

    [HttpPost("register")]
    public async Task<ActionResult<Result<string>>> Register([FromBody] RegisterRequest request)
    {
        // Implementation will go here
        return Ok(Result<string>.Success("Registration successful"));
    }
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FirstName, string LastName); 