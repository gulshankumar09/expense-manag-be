using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using SharedLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Services;

/// <summary>
/// Service for handling authentication and authorization operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the AuthService
    /// </summary>
    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        IOtpService otpService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _otpService = otpService;
        _emailService = emailService;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<Result<AuthResponse>> GoogleLoginAsync(string email, string googleId, string firstName, string lastName)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser == null)
        {
            var newUser = User.CreateWithGoogle(email, googleId, firstName, lastName);
            var result = await _userManager.CreateAsync(newUser);
            
            if (!result.Succeeded)
                return Result<AuthResponse>.Failure(result.Errors.First().Description);

            await _userManager.AddToRoleAsync(newUser, "User");
            existingUser = newUser;
        }
        else
        {
            if (string.IsNullOrEmpty(existingUser.GoogleId))
            {
                existingUser.GoogleId = googleId;
                existingUser.EmailConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(existingUser);
                
                if (!updateResult.Succeeded)
                    return Result<AuthResponse>.Failure(updateResult.Errors.First().Description);
            }
            else if (existingUser.GoogleId != googleId)
            {
                return Result<AuthResponse>.Failure("Email is already registered with different credentials");
            }
        }

        var authResponse = await GenerateAuthResponse(existingUser);
        return Result<AuthResponse>.Success(authResponse);
    }

    /// <inheritdoc/>
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

        var authResponse = await GenerateAuthResponse(user);
        return Result<AuthResponse>.Success(authResponse);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => 
            u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return Result<AuthResponse>.Failure("Invalid or expired refresh token");

        var authResponse = await GenerateAuthResponse(user);
        return Result<AuthResponse>.Success(authResponse);
    }

    /// <inheritdoc/>
    public async Task<Result<string>> RevokeTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<string>.Failure("User not found");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return Result<string>.Success("Token revoked successfully");
    }

    /// <inheritdoc/>
    public async Task<Result<string>> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<string>.Failure("User not found");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.First().Description);

        return Result<string>.Success("Password changed successfully");
    }

    /// <summary>
    /// Generates authentication response including JWT token and refresh token
    /// </summary>
    /// <param name="user">The user to generate tokens for</param>
    /// <returns>Authentication response containing tokens and user information</returns>
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
} 