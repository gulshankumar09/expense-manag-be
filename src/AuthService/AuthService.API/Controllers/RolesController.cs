using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Constants;

namespace AuthService.API.Controllers;

/// <summary>
/// Controller for managing roles and role assignments
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireSuperAdminRole")]
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
    /// <response code="409">Returns when role already exists</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await _roleService.CreateRoleAsync(request);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                Error { Code: ErrorConstants.Codes.ConflictCode } => Conflict(result),
                Error { Code: ErrorConstants.Codes.UnauthorizedCode } => Unauthorized(result),
                _ => BadRequest(result)
            };
        }

        return Ok(result);
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
    /// <response code="409">Returns when user already has the role</response>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result>> AssignRole([FromBody] AssignRoleRequest request)
    {
        var result = await _roleService.AssignRoleAsync(request);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                Error { Code: ErrorConstants.Codes.NotFoundCode } => NotFound(result),
                Error { Code: ErrorConstants.Codes.UnauthorizedCode } => Unauthorized(result),
                Error { Code: ErrorConstants.Codes.ConflictCode } => Conflict(result),
                _ => BadRequest(result)
            };
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates the maximum number of allowed SuperAdmin users
    /// </summary>
    /// <param name="newLimit">The new maximum limit for SuperAdmin users</param>
    /// <returns>A success message if limit is updated successfully</returns>
    /// <response code="200">Returns success message when limit is updated</response>
    /// <response code="400">Returns error message when update fails</response>
    /// <response code="401">Returns when user is not authenticated</response>
    /// <response code="403">Returns when user is not authorized (not a SuperAdmin)</response>
    [HttpPut("superadmin-limit")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> UpdateSuperAdminLimit([FromBody] int newLimit)
    {
        var result = await _roleService.UpdateSuperAdminLimitAsync(newLimit);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Retrieves a list of all roles in the system
    /// </summary>
    /// <returns>A list of roles with their details</returns>
    /// <response code="200">Returns the list of roles</response>
    /// <response code="401">Returns when user is not authenticated</response>
    /// <response code="403">Returns when user is not authorized (not a SuperAdmin)</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(Result<ListOfRolesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<ListOfRolesResponse>>> ListRoles()
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<IEnumerable<string>>>> GetUserRoles(string userId)
    {
        var result = await _roleService.GetUserRolesAsync(userId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}