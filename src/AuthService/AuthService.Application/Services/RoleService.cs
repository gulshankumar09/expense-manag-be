using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using SharedLibrary.Models;
using AuthService.Application.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthService.Application.Services;

/// <summary>
/// Service implementation for managing roles and role assignments
/// </summary>
public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RoleService> _logger;
    private readonly RoleSettings _roleSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the RoleService
    /// </summary>
    /// <param name="roleManager">The ASP.NET Identity role manager</param>
    /// <param name="userManager">The ASP.NET Identity user manager</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="roleSettings">The role settings</param>
    /// <param name="httpContextAccessor">The HTTP context accessor</param>
    public RoleService(
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager,
        ILogger<RoleService> logger,
        IOptions<RoleSettings> roleSettings,
        IHttpContextAccessor httpContextAccessor)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
        _roleSettings = roleSettings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<bool> IsCurrentUserAuthorizedForRole(string roleToAssign)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return false;

        var currentUser = await _userManager.FindByIdAsync(currentUserId);
        if (currentUser == null)
            return false;

        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

        // SuperAdmin can assign any role
        if (currentUserRoles.Contains("SuperAdmin"))
            return true;

        // Admin can only assign non-SuperAdmin roles
        if (currentUserRoles.Contains("Admin"))
            return roleToAssign != "SuperAdmin";

        return false;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<Result<string>> AssignRoleAsync(AssignRoleRequest request)
    {
        try
        {
            // Check if current user is authorized to assign this role
            if (!await IsCurrentUserAuthorizedForRole(request.RoleName))
            {
                return Result<string>.Failure("You are not authorized to assign this role");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result<string>.Failure("User not found");

            if (!await _roleManager.RoleExistsAsync(request.RoleName))
                return Result<string>.Failure("Role not found");

            // Special handling for SuperAdmin role
            if (request.RoleName == "SuperAdmin")
            {
                var currentSuperAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                if (currentSuperAdmins.Count >= _roleSettings.MaxSuperAdminUsers && !currentSuperAdmins.Any(u => u.Id == request.UserId))
                {
                    return Result<string>.Failure($"Cannot assign SuperAdmin role. Maximum limit of {_roleSettings.MaxSuperAdminUsers} SuperAdmin user(s) has been reached.");
                }
            }

            // Special handling for Admin role
            if (request.RoleName == "Admin")
            {
                var currentUser = await _userManager.FindByIdAsync(_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (currentUser == null)
                    return Result<string>.Failure("Current user not found");

                var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
                if (!currentUserRoles.Contains("SuperAdmin") && !currentUserRoles.Contains("Admin"))
                {
                    return Result<string>.Failure("Only SuperAdmin or Admin users can assign the Admin role");
                }
            }

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

    /// <inheritdoc/>
    public async Task<Result<string>> UpdateSuperAdminLimitAsync(int newLimit)
    {
        try
        {
            var currentSuperAdminCount = (await _userManager.GetUsersInRoleAsync("SuperAdmin")).Count;
            if (currentSuperAdminCount > newLimit)
                return Result<string>.Failure($"Cannot update limit to {newLimit} as there are already {currentSuperAdminCount} SuperAdmin users.");

            _roleSettings.MaxSuperAdminUsers = newLimit;
            _logger.LogInformation("SuperAdmin user limit updated to {NewLimit}", newLimit);
            return Result<string>.Success($"SuperAdmin user limit updated to {newLimit}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SuperAdmin user limit");
            return Result<string>.Failure("An error occurred while updating the SuperAdmin user limit");
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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