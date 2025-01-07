using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using SharedLibrary.Models;

namespace AuthService.API.Controllers;

/// <summary>
/// Controller for managing roles and role assignments
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can manage roles
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    /// <summary>
    /// Initializes a new instance of the RolesController
    /// </summary>
    /// <param name="roleService">The role management service</param>
    /// <param name="logger">The logger instance</param>
    public RolesController(
        IRoleService roleService,
        ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new role in the system
    /// </summary>
    /// <param name="request">The role creation request containing the role name</param>
    /// <returns>A success message if role is created successfully</returns>
    /// <response code="200">Returns success message when role is created</response>
    /// <response code="400">Returns error message when role creation fails</response>
    /// <response code="401">Returns when user is not authenticated</response>
    /// <response code="403">Returns when user is not authorized (not a SuperAdmin)</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<string>>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await _roleService.CreateRoleAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Assigns a role to a user
    /// </summary>
    /// <param name="request">The role assignment request containing user ID and role name</param>
    /// <returns>A success message if role is assigned successfully</returns>
    /// <response code="200">Returns success message when role is assigned</response>
    /// <response code="400">Returns error message when assignment fails</response>
    /// <response code="401">Returns when user is not authenticated</response>
    /// <response code="403">Returns when user is not authorized (not a SuperAdmin)</response>
    /// <response code="404">Returns when user or role is not found</response>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<string>>> AssignRole([FromBody] AssignRoleRequest request)
    {
        var result = await _roleService.AssignRoleAsync(request);
        if (!result.IsSuccess)
        {
            return result.Error.Contains("not found") 
                ? NotFound(result) 
                : BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Updates the maximum number of allowed SuperAdmin users
    /// </summary>
    /// <param name="newLimit">The new maximum limit for SuperAdmin users</param>
    /// <returns>A success message if the limit is updated successfully</returns>
    /// <response code="200">Returns success message when limit is updated</response>
    /// <response code="400">Returns error message when update fails</response>
    /// <response code="401">Returns when user is not authenticated</response>
    /// <response code="403">Returns when user is not authorized (not a SuperAdmin)</response>
    [HttpPost("superadmin-limit")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<string>>> UpdateSuperAdminLimit([FromBody] int newLimit)
    {
        if (newLimit < 1)
            return BadRequest(Result<string>.Failure("SuperAdmin limit must be at least 1"));

        var result = await _roleService.UpdateSuperAdminLimitAsync(newLimit);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Retrieves a list of all roles in the system
    /// </summary>
    /// <returns>A list of role names</returns>
    /// <response code="200">Returns the list of roles</response>
    /// <response code="401">Returns when user is not authenticated</response>
    /// <response code="403">Returns when user is not authorized (not a SuperAdmin)</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(Result<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<IEnumerable<string>>>> ListRoles()
    {
        var result = await _roleService.ListRolesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all roles assigned to a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A list of role names assigned to the user</returns>
    /// <response code="200">Returns the list of user's roles</response>
    /// <response code="401">Returns when user is not authenticated</response>
    /// <response code="403">Returns when user is not authorized (not a SuperAdmin)</response>
    /// <response code="404">Returns when user is not found</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(Result<IEnumerable<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<IEnumerable<string>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<IEnumerable<string>>>> GetUserRoles(string userId)
    {
        var result = await _roleService.GetUserRolesAsync(userId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
} 