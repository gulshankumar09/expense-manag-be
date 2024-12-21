using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using SharedLibrary.Models;

namespace AuthService.Application.Interfaces;

public interface IUserService
{
    Task<Result<User>> GetUserByIdAsync(Guid id);
    Task<Result<User>> GetUserByEmailAsync(string email);
    Task<Result<string>> RegisterUserAsync(RegisterRequest request);
    Task<Result<AuthResponse>> VerifyOtpAsync(VerifyOtpRequest request);
    Task<Result<AuthResponse>> GoogleLoginAsync(string email, string googleId, string firstName, string lastName);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<Result<string>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}