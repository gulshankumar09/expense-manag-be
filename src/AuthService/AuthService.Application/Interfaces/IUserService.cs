using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using SharedLibrary.Models;

namespace AuthService.Application.Interfaces;

public interface IUserService
{
    Task<Result<User>> GetUserByIdAsync(Guid id);
    Task<Result<User>> GetUserByEmailAsync(string email);
    Task<Result<string>> RegisterUserAsync(string email, string password, string firstName, string lastName, string phoneNumber);
    Task<Result<AuthResponse>> VerifyOtpAsync(string email, string otp);
    Task<Result<AuthResponse>> GoogleLoginAsync(string email, string googleId, string firstName, string lastName);
}
