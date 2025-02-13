using System.ComponentModel.DataAnnotations;
using AuthService.Domain.ValueObjects;
using SharedLibrary.Validation;

namespace AuthService.Application.DTOs;

public record RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [PasswordValidation]
    public string Password { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; init; } = string.Empty;

    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; init; } = string.Empty;

    [Phone]
    public string PhoneNumber { get; init; } = string.Empty;
}

public record LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record VerifyEmailRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Token { get; init; } = string.Empty;
}

public record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;
}

public record ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Token { get; init; } = string.Empty;

    [Required]
    [PasswordValidation(
        minLength: 8,
        maxLength: 128,
        requireDigit: true,
        requireLowercase: true,
        requireUppercase: true,
        requireSpecialChar: true)]
    public string NewPassword { get; init; } = string.Empty;
}

public record VerifyOtpRequest
{
    [Required]
    [EmailAddress]
    [NoXss]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Otp { get; init; } = string.Empty;
}

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

public record CreateRoleRequest
{
    [Required]
    [NoXss]
    public string Name { get; init; } = string.Empty;
}

public record AssignRoleRequest
{
    [Required]
    public string UserId { get; init; } = string.Empty;

    [Required]
    [NoXss]
    public string RoleName { get; init; } = string.Empty;
}

public record UpdateUserRequest
{
    [Required]
    [NoXss]
    public string FirstName { get; init; } = string.Empty;

    [NoXss]
    public string LastName { get; init; } = string.Empty;

    [NoXss]
    public string PhoneNumber { get; init; } = string.Empty;
}

public record ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [PasswordValidation(
        minLength: 8,
        maxLength: 128,
        requireDigit: true,
        requireLowercase: true,
        requireUppercase: true,
        requireSpecialChar: true)]
    public string NewPassword { get; init; } = string.Empty;
}

public record RoleDto(
    string Id,
    string Name,
    string NormalizedName,
    int UsersCount);

public record ListOfRolesResponse(
    IEnumerable<string> Roles);

public record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}