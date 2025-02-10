namespace AuthService.Application.DTOs;

public record UserListRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? Role = null,
    string? SortBy = null,
    bool SortDescending = false);