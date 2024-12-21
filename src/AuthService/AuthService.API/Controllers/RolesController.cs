using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using AuthService.Domain.Entities;
using AuthService.Application.DTOs;
using SharedLibrary.Models;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RolesController> _logger;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager,
        ILogger<RolesController> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult<Result<string>>> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (await _roleManager.RoleExistsAsync(request.Name))
            return BadRequest(Result<string>.Failure("Role already exists"));

        var result = await _roleManager.CreateAsync(new IdentityRole(request.Name));
        if (!result.Succeeded)
            return BadRequest(Result<string>.Failure(result.Errors.First().Description));

        return Ok(Result<string>.Success($"Role '{request.Name}' created successfully"));
    }

    [HttpPost("assign")]
    public async Task<ActionResult<Result<string>>> AssignRole([FromBody] AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return NotFound(Result<string>.Failure("User not found"));

        if (!await _roleManager.RoleExistsAsync(request.RoleName))
            return NotFound(Result<string>.Failure("Role not found"));

        if (await _userManager.IsInRoleAsync(user, request.RoleName))
            return BadRequest(Result<string>.Failure("User is already in this role"));

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
            return BadRequest(Result<string>.Failure(result.Errors.First().Description));

        return Ok(Result<string>.Success($"Role '{request.RoleName}' assigned to user successfully"));
    }

    [HttpGet("list")]
    public ActionResult<Result<IEnumerable<string>>> ListRoles()
    {
        var roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return Ok(Result<IEnumerable<string>>.Success(roles!));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<Result<IEnumerable<string>>>> GetUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(Result<IEnumerable<string>>.Failure("User not found"));

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(Result<IEnumerable<string>>.Success(roles));
    }
} 