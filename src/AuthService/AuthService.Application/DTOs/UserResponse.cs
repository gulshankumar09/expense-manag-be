using AuthService.Domain.ValueObjects;

namespace AuthService.Application.DTOs;

public record UserResponse(
    string Id,
    string Email,
    PersonName Name,
    string PhoneNumber,
    bool IsEmailVerified,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<string> Roles);