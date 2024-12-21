using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using SharedLibrary.Models;

namespace AuthService.Application.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager,
        ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<string>> CreateRoleAsync(CreateRoleRequest request)
    {
        try
        {
            if (await _roleManager.RoleExistsAsync(request.Name))
                return Result<string>.Failure("Role already exists");

            var result = await _roleManager.CreateAsync(new IdentityRole(request.Name));
            if (!result.Succeeded)
                return Result<string>.Failure(result.Errors.First().Description);

            _logger.LogInformation("Role '{RoleName}' created successfully", request.Name);
            return Result<string>.Success($"Role '{request.Name}' created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role '{RoleName}'", request.Name);
            return Result<string>.Failure("An error occurred while creating the role");
        }
    }

    public async Task<Result<string>> AssignRoleAsync(AssignRoleRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result<string>.Failure("User not found");

            if (!await _roleManager.RoleExistsAsync(request.RoleName))
                return Result<string>.Failure("Role not found");

            if (await _userManager.IsInRoleAsync(user, request.RoleName))
                return Result<string>.Failure("User is already in this role");

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
                return Result<string>.Failure(result.Errors.First().Description);

            _logger.LogInformation("Role '{RoleName}' assigned to user '{UserId}' successfully", request.RoleName, request.UserId);
            return Result<string>.Success($"Role '{request.RoleName}' assigned to user successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role '{RoleName}' to user '{UserId}'", request.RoleName, request.UserId);
            return Result<string>.Failure("An error occurred while assigning the role");
        }
    }

    public async Task<Result<IEnumerable<string>>> ListRolesAsync()
    {
        try
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Result<IEnumerable<string>>.Success(roles!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing roles");
            return Result<IEnumerable<string>>.Failure("An error occurred while listing roles");
        }
    }

    public async Task<Result<IEnumerable<string>>> GetUserRolesAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<IEnumerable<string>>.Failure("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            return Result<IEnumerable<string>>.Success(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user '{UserId}'", userId);
            return Result<IEnumerable<string>>.Failure("An error occurred while getting user roles");
        }
    }
} 