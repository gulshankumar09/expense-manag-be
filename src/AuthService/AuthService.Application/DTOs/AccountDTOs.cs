using AuthService.Domain.ValueObjects;

namespace AuthService.Application.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber);

public record LoginRequest(
    string Email,
    string Password);

public record VerifyEmailRequest(
    string Email,
    string Token);

public record ForgotPasswordRequest(
    string Email);

public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);

public record VerifyOtpRequest(
    string Email,
    string Otp);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    string Id,
    string Email,
    PersonName Name,
    bool IsEmailVerified,
    IList<string> Roles);

public record CreateRoleRequest(
    string Name);

public record AssignRoleRequest(
    string UserId,
    string RoleName);

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string PhoneNumber);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword); 