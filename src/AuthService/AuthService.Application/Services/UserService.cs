using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using SharedLibrary.Models;

namespace AuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
        if (existingUser != null)
            return Result<string>.Failure("Email already registered");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = User.Create(email, passwordHash, firstName, lastName);
        user.SetPhoneNumber(phoneNumber);
        
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        
        return Result<string>.Success("User registered successfully");
    }

    public Task<Result<AuthResponse>> VerifyOtpAsync(string email, string otp)
    {
        throw new NotImplementedException();
    }

    public Task<Result<AuthResponse>> GoogleLoginAsync(string email, string googleId, string firstName, string lastName)
    {
        throw new NotImplementedException();
    }
} 