using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using SharedLibrary.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
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

    public async Task<Result<string>> RegisterUserAsync(string email, string password, string firstName, string lastName, string phoneNumber)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null && !existingUser.IsEmailVerified)
            return Result<string>.Success("Email already registered, Please Verify your email");

        if (existingUser != null && existingUser.IsEmailVerified)
            return Result<string>.Failure("Email already registered and verified");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = User.Create(email, passwordHash, firstName, lastName);
        user.SetPhoneNumber(phoneNumber);
        
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        
        return Result<string>.Success("User registered successfully");
    }

    public async Task<Result<AuthResponse>> VerifyOtpAsync(string email, string otp)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return Result<AuthResponse>.Failure("User not found");

        if (user.IsEmailVerified)
            return Result<AuthResponse>.Failure("Email is already verified");

        if (user.VerificationToken != otp || !user.VerificationTokenExpiry.HasValue || 
            user.VerificationTokenExpiry.Value < DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Invalid or expired OTP");

        user.VerifyEmail();
        
        // Generate tokens
        var accessToken = GenerateJwtToken(user);
        var refreshToken = Guid.NewGuid().ToString();
        user.SetRefreshToken(refreshToken, TimeSpan.FromDays(7));

        await _userRepository.SaveChangesAsync();

        var response = new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
            User: new UserDto(
                Id: user.Id,
                Email: user.Email.Value,
                FirstName: user.Name.FirstName,
                LastName: user.Name.LastName,
                IsEmailVerified: user.IsEmailVerified
            )
        );

        return Result<AuthResponse>.Success(response);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email.Value),
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

    public Task<Result<AuthResponse>> GoogleLoginAsync(string email, string googleId, string firstName, string lastName)
    {
        throw new NotImplementedException();
    }
} 