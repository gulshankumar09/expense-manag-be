using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Models;

namespace AuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public UserService(
        IUserRepository userRepository,
        UserManager<User> userManager,
        IOtpService otpService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _otpService = otpService;
        _emailService = emailService;
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
            CreatedBy = "SYSTEM_USER"
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

    public async Task<Result<string>> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<string>.Failure("User not found");

        user.Name = PersonName.Create(request.FirstName, request.LastName);
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = userId;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.First().Description);

        return Result<string>.Success("User updated successfully");
    }

    public async Task<Result<string>> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<string>.Failure("User not found");

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = userId;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.First().Description);

        return Result<string>.Success("User deleted successfully");
    }

    public async Task<Result<string>> UpdateUserRolesAsync(string userId, IEnumerable<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<string>.Failure("User not found");

        var currentRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.First().Description);

        result = await _userManager.AddToRolesAsync(user, roles);
        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.First().Description);

        return Result<string>.Success("User roles updated successfully");
    }
}