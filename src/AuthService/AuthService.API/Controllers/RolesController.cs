using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using SharedLibrary.Models;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(
        IRoleService roleService,
        ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<string>>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await _roleService.CreateRoleAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

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

    [HttpGet("list")]
    [ProducesResponseType(typeof(Result<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<IEnumerable<string>>>> ListRoles()
    {
        var result = await _roleService.ListRolesAsync();
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(Result<IEnumerable<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<IEnumerable<string>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<IEnumerable<string>>>> GetUserRoles(string userId)
    {
        var result = await _roleService.GetUserRolesAsync(userId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
} 