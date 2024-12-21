using AuthService.Application.DTOs;
using SharedLibrary.Models;

namespace AuthService.Application.Interfaces;

public interface IRoleService
{
    Task<Result<string>> CreateRoleAsync(CreateRoleRequest request);
    Task<Result<string>> AssignRoleAsync(AssignRoleRequest request);
    Task<Result<IEnumerable<string>>> ListRolesAsync();
    Task<Result<IEnumerable<string>>> GetUserRolesAsync(string userId);
} 