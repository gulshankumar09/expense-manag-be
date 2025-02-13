using AuthService.Application.Configuration;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Models;
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
    private readonly RoleConfiguration _roleConfig;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRoleSettingsRepository _roleSettingsRepository;

    /// <summary>
    /// Initializes a new instance of the RoleService
    /// </summary>
    /// <param name="roleManager">The ASP.NET Identity role manager</param>
    /// <param name="userManager">The ASP.NET Identity user manager</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="roleConfig">The role configuration</param>
    /// <param name="httpContextAccessor">The HTTP context accessor</param>
    /// <param name="roleSettingsRepository">The role settings repository</param>
    public RoleService(
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager,
        ILogger<RoleService> logger,
        IOptions<RoleConfiguration> roleConfig,
        IHttpContextAccessor httpContextAccessor,
        IRoleSettingsRepository roleSettingsRepository)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
        _roleConfig = roleConfig.Value;
        _httpContextAccessor = httpContextAccessor;
        _roleSettingsRepository = roleSettingsRepository;
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
    public async Task<IResult> CreateRoleAsync(CreateRoleRequest request)
    {
        try
        {
            if (await _roleManager.RoleExistsAsync(request.Name))
                return Result.Failure(Error.Conflict());

            var result = await _roleManager.CreateAsync(new IdentityRole(request.Name));
            if (!result.Succeeded)
                return Result.Failure(Error.BadRequest(result.Errors.First().Description));

            _logger.LogInformation("Role '{RoleName}' created successfully", request.Name);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role '{RoleName}'", request.Name);
            return Result.Failure(Error.InternalServerError());
        }
    }

    /// <inheritdoc/>
    public async Task<IResult> AssignRoleAsync(AssignRoleRequest request)
    {
        try
        {
            if (!await IsCurrentUserAuthorizedForRole(request.RoleName))
                return Result.Failure(Error.Unauthorized());

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result.Failure(Error.NotFound());

            if (!await _roleManager.RoleExistsAsync(request.RoleName))
                return Result.Failure(Error.NotFound());

            // Special handling for SuperAdmin role
            if (request.RoleName == "SuperAdmin")
            {
                var currentSuperAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                if (currentSuperAdmins.Count >= _roleConfig.MaxSuperAdminUsers && !currentSuperAdmins.Any(u => u.Id == request.UserId))
                {
                    return Result.Failure(Error.BadRequest($"Cannot assign SuperAdmin role. Maximum limit of {_roleConfig.MaxSuperAdminUsers} SuperAdmin user(s) has been reached."));
                }
            }

            if (await _userManager.IsInRoleAsync(user, request.RoleName))
                return Result.Failure(Error.Conflict());

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
                return Result.Failure(Error.BadRequest(result.Errors.First().Description));

            _logger.LogInformation("Role '{RoleName}' assigned to user '{UserId}' successfully", request.RoleName, request.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role '{RoleName}' to user '{UserId}'", request.RoleName, request.UserId);
            return Result.Failure(Error.InternalServerError());
        }
    }

    /// <inheritdoc/>
    public async Task<IResult> UpdateSuperAdminLimitAsync(int newLimit)
    {
        try
        {
            var currentSuperAdminCount = (await _userManager.GetUsersInRoleAsync("SuperAdmin")).Count;
            if (currentSuperAdminCount > newLimit)
                return Result.Failure(Error.BadRequest($"Cannot update limit to {newLimit} as there are already {currentSuperAdminCount} SuperAdmin users."));

            // Update in-memory configuration
            _roleConfig.MaxSuperAdminUsers = newLimit;

            // Update database settings
            var settings = await _roleSettingsRepository.GetSettingsAsync();
            settings.MaxSuperAdminUsers = newLimit;
            await _roleSettingsRepository.UpdateSettingsAsync(settings);

            _logger.LogInformation("SuperAdmin user limit updated to {NewLimit}", newLimit);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SuperAdmin user limit");
            return Result.Failure(Error.InternalServerError());
        }
    }

    /// <inheritdoc/>
    public async Task<IResult<ListOfRolesResponse>> ListRolesAsync()
    {
        try
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var response = new ListOfRolesResponse(roles!);
            return Result<ListOfRolesResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing roles");
            return Result<ListOfRolesResponse>.Failure(Error.InternalServerError());
        }
    }

    /// <inheritdoc/>
    public async Task<IResult<IEnumerable<string>>> GetUserRolesAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<IEnumerable<string>>.Failure(Error.NotFound());

            var roles = await _userManager.GetRolesAsync(user);
            return Result<IEnumerable<string>>.Success(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user '{UserId}'", userId);
            return Result<IEnumerable<string>>.Failure(Error.InternalServerError());
        }
    }
}