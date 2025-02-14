using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Models;
using SharedLibrary.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SharedLibrary.Utility;
using AuthService.Application.Constants;

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
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the AuthService
    /// </summary>
    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        IOtpService otpService,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _otpService = otpService;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public async Task<IResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponse>.Failure(Error.Unauthorized());

        if (!user.EmailConfirmed)
            return Result<AuthResponse>.Failure(Error.BadRequest("Please verify your email first"));

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Result<AuthResponse>.Failure(Error.BadRequest("Account is locked. Try again later."));

            return Result<AuthResponse>.Failure(Error.Unauthorized());
        }

        var authResponse = await GenerateAuthResponse(user);
        return Result<AuthResponse>.Success(authResponse);
    }

    /// <inheritdoc/>
    public async Task<IResult<AuthResponse>> GoogleLoginAsync(string email, string googleId, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser == null)
        {
            var newUser = new User
            {
                UserName = email,
                Email = email,
                GoogleId = googleId,
                Name = PersonName.Create(firstName, lastName),
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "SYSTEM"
            };

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
                return Result<AuthResponse>.Failure(Error.BadRequest(result.Errors.First().Description));

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
                    return Result<AuthResponse>.Failure(Error.BadRequest(updateResult.Errors.First().Description));
            }
            else if (existingUser.GoogleId != googleId)
            {
                return Result<AuthResponse>.Failure(Error.BadRequest("Email is already registered with different credentials"));
            }
        }

        var authResponse = await GenerateAuthResponse(existingUser);
        return Result<AuthResponse>.Success(authResponse);
    }

    /// <inheritdoc/>
    public async Task<Result<AuthResponse>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
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
    public async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Success(); // Don't reveal that the user doesn't exist

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"https://yourapp.com/reset-password?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendEmailAsync(
            request.Email,
            "Reset Your Password",
            $"Click the following link to reset your password: {resetLink}");

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result.Failure(Error.BadRequest("Invalid request"));

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return Result.Failure(Error.BadRequest(result.Errors.First().Description));

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<IResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == request.RefreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return Result<AuthResponse>.Failure(Error.Unauthorized());

        var authResponse = await GenerateAuthResponse(user);
        return Result<AuthResponse>.Success(authResponse);
    }

    /// <inheritdoc/>
    public async Task<Result<string>> RevokeTokenAsync(string userId, CancellationToken cancellationToken = default)
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
    public async Task<IResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Result.Failure(Error.Unauthorized());

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(Error.NotFound());

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return Result.Failure(Error.BadRequest(result.Errors.First().Description));

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<IResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var redirectUrl = UrlUtility.GetAbsoluteUrl(_httpContextAccessor.HttpContext.Request,ApiEndpoints.Account.VerifyOtp);
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            if (!existingUser.EmailConfirmed)
            {
                // Generate and send new OTP
                var newOtp = _otpService.GenerateOtp();
                existingUser.SetVerificationToken(newOtp, TimeSpan.FromMinutes(15));
                await _userManager.UpdateAsync(existingUser);
                await _otpService.SendOtpAsync(request.Email, newOtp);

                var result = Result<AuthResponse>.Failure(Error.BadRequest("Email already registered. Please verify your email."));
                result.AddHeader(HeaderKeys.RedirectUrl, redirectUrl);
                return result;
            }

            return Result.Failure(Error.Conflict(ErrorConstants.Messages.EmailAlreadyExists));
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Name = PersonName.Create(request.FirstName, request.LastName),
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM"
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Result.Failure(Error.BadRequest(createResult.Errors.First().Description));

        try
        {
            // Add to User role by default
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                // If role assignment fails, delete the user and return error
                await _userManager.DeleteAsync(user);
                return Result.Failure(Error.BadRequest("Failed to assign User role. Registration cancelled."));
            }

            // Generate and send OTP
            var registrationOtp = _otpService.GenerateOtp();
            user.SetVerificationToken(registrationOtp, TimeSpan.FromMinutes(15));
            await _userManager.UpdateAsync(user);
            await _otpService.SendOtpAsync(request.Email, registrationOtp);

            var result = Result.Success();
            result.AddHeader(HeaderKeys.RedirectUrl, redirectUrl);
            return result;
        }
        catch (Exception)
        {
            // If anything fails after user creation, clean up by deleting the user
            await _userManager.DeleteAsync(user);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IResult> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u =>
            u.VerificationToken == token &&
            u.VerificationTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return Result.Failure(Error.BadRequest("Invalid or expired verification token"));

        if (user.EmailConfirmed)
            return Result.Failure(Error.BadRequest("Email is already verified"));

        user.VerifyEmail();
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<IResult> LogoutAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(Error.NotFound());

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return Result.Success();
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