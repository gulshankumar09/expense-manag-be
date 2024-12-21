using Microsoft.AspNetCore.Mvc;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using SharedLibrary.Models;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<AccountController> _logger;
    private readonly IUserService _userService;
    public AccountController(
        IConfiguration configuration,
        IEmailService emailService,
        IOtpService otpService,
        IUserService userService,
        ILogger<AccountController> logger)
    {
        _configuration = configuration;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
        _userService = userService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<string>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterUserAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<AuthResponse>>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _userService.VerifyOtpAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<Result<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _userService.ForgotPasswordAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<Result<string>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _userService.ResetPasswordAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}