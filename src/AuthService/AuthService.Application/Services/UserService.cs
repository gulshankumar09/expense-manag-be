using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using SharedLibrary.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using AuthService.Domain.ValueObjects;

namespace AuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public UserService(
        IUserRepository userRepository, 
        IConfiguration configuration,
        UserManager<User> userManager,
        IOtpService otpService,
        IEmailService emailService,
        SignInManager<User> signInManager)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _userManager = userManager;
        _otpService = otpService;
        _emailService = emailService;
        _signInManager = signInManager;
    }

    public async Task<Result<User>> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null 
            ? Result<User>.Success(user) 
            : Result<User>.Failure("User not found");
    }

    public async Task<Result<User>> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null 
            ? Result<User>.Success(user) 
            : Result<User>.Failure("User not found");
    }

    public async Task<Result<string>> RegisterUserAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            if (!existingUser.EmailConfirmed)
                return Result<string>.Success("Email already registered. Please verify your email.");
            
            return Result<string>.Failure("Email already registered and verified.");
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Name = PersonName.Create(request.FirstName, request.LastName),
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.First().Description);
        
        // Add to User role by default
        await _userManager.AddToRoleAsync(user, "User");

        // Generate and send OTP
        var otp = _otpService.GenerateOtp();
        user.SetVerificationToken(otp, TimeSpan.FromMinutes(15));
        await _userManager.UpdateAsync(user);
        await _otpService.SendOtpAsync(request.Email, otp);

        return Result<string>.Success("Registration successful. Please verify your email.");
    }

    public async Task<Result<AuthResponse>> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponse>.Failure("User not found");

        if (user.EmailConfirmed)
            return Result<AuthResponse>.Failure("Email is already verified");

        if (user.VerificationToken != request.Otp || !user.VerificationTokenExpiry.HasValue || 
            user.VerificationTokenExpiry.Value < DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Invalid or expired OTP");

        user.VerifyEmail();
        await _userManager.UpdateAsync(user);
        
        // // Generate tokens
        // var accessToken = GenerateJwtToken(user);
        // var refreshToken = Guid.NewGuid().ToString();
        // user.SetRefreshToken(refreshToken, TimeSpan.FromDays(7));

        // await _userRepository.SaveChangesAsync();

        // var response = new AuthResponse(
        //     AccessToken: accessToken,
        //     RefreshToken: refreshToken,
        //     ExpiresAt: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
        //     User: new UserDto(
        //         Id: user.Id,
        //         Email: user.Email ?? string.Empty,
        //         FirstName: user.Name.FirstName,
        //         LastName: user.Name.LastName,
        //         IsEmailVerified: user.EmailConfirmed,
        //         Roles: new List<string>()
        //     )
        // );

        var authResponse = await GenerateAuthResponse(user);
        return Result<AuthResponse>.Success(authResponse);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"] ?? "60"));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new("userId", user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.Name.FirstName),
            new(ClaimTypes.Surname, user.Name.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, string.Join(", ", roles))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? 
            throw new InvalidOperationException("JWT Key not found in configuration")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"] ?? "60"));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var refreshToken = Guid.NewGuid().ToString();
        user.SetRefreshToken(refreshToken, TimeSpan.FromDays(7));
        await _userManager.UpdateAsync(user);

        return new AuthResponse(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken: refreshToken,
            ExpiresAt: expires,
            User: new UserDto(
                Id: user.Id,
                Email: user.Email!,
                Name: PersonName.Create(user.Name.FirstName, user.Name.LastName),
                IsEmailVerified: user.EmailConfirmed,
                Roles: roles.ToList()
            )
        );
    }

    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<string>.Success("If your email is registered, you will receive a password reset link.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"https://yourapp.com/reset-password?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendEmailAsync(
            request.Email,
            "Reset Your Password",
            $"Click the following link to reset your password: {resetLink}");

        return Result<string>.Success("Password reset link sent to your email.");
    }

    public Task<Result<AuthResponse>> GoogleLoginAsync(string email, string googleId, string firstName, string lastName)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<string>.Failure("Invalid request");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.First().Description);

        return Result<string>.Success("Password has been reset successfully.");
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponse>.Failure("Invalid credentials");

        if (!user.EmailConfirmed)
            return Result<AuthResponse>.Failure("Please verify your email first");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Result<AuthResponse>.Failure("Account is locked. Try again later.");
            
            return Result<AuthResponse>.Failure("Invalid credentials");
        }

        var authResponse = await GenerateAuthResponse(user);
        return Result<AuthResponse>.Success(authResponse);
    }
} 