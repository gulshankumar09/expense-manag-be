namespace AuthService.Application.DTOs;

public record RegisterRequest(
    string Email, 
    string Password, 
    string FirstName, 
    string LastName,
    string PhoneNumber);

public record VerifyOtpRequest(
    string Email, 
    string Otp);

public record LoginRequest(
    string Email, 
    string Password);

public record GoogleAuthRequest(
    string IdToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsEmailVerified); 