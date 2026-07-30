using System.Threading.RateLimiting;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Auth;
using LpgErp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request.Username, request.Password, ip);
        if (!result.IsSuccess) return Unauthorized(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse<AuthResult>.Ok(result));
    }

    [HttpPost("register")]
    [Authorize(Policy = AppPermissions.Users.Create)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RegisterAsync(request, ip);
        if (!result.IsSuccess) return BadRequest(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse<AuthResult>.Ok(result));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(request.Token, ip);
        if (!result.IsSuccess) return Unauthorized(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse<AuthResult>.Ok(result));
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _authService.RevokeTokenAsync(request.Token, ip);
        return Ok(ApiResponse.Ok("Token revoked."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        if (userId == null) return Unauthorized();

        var user = await _authService.GetUserAsync(Guid.Parse(userId));
        if (user == null) return NotFound();
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpGet("users")]
    [Authorize(Policy = AppPermissions.Users.View)]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var users = await _authService.GetUsersAsync(pageNumber, pageSize);
        return Ok(ApiResponse<List<UserDto>>.Ok(users));
    }

    [HttpGet("users/{id:guid}")]
    [Authorize(Policy = AppPermissions.Users.View)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _authService.GetUserAsync(id);
        if (user == null) return NotFound();
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpPost("users")]
    [Authorize(Policy = AppPermissions.Users.Create)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _authService.CreateUserAsync(request);
        if (user == null) return BadRequest(ApiResponse.Fail("Username or email already exists."));
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpPut("users/{id:guid}")]
    [Authorize(Policy = AppPermissions.Users.Edit)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _authService.UpdateUserAsync(id, request);
        if (user == null) return NotFound();
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpDelete("users/{id:guid}")]
    [Authorize(Policy = AppPermissions.Users.Delete)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var success = await _authService.DeleteUserAsync(id);
        if (!success) return NotFound();
        return Ok(ApiResponse.Ok("User deleted."));
    }

    [HttpPost("users/{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Policy = AppPermissions.Users.ManageRoles)]
    public async Task<IActionResult> AssignRole(Guid userId, Guid roleId)
    {
        var success = await _authService.AssignRoleAsync(userId, roleId);
        if (!success) return BadRequest(ApiResponse.Fail("Role assignment failed."));
        return Ok(ApiResponse.Ok("Role assigned."));
    }

    [HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Policy = AppPermissions.Users.ManageRoles)]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId)
    {
        var success = await _authService.RemoveRoleAsync(userId, roleId);
        if (!success) return BadRequest(ApiResponse.Fail("Role removal failed."));
        return Ok(ApiResponse.Ok("Role removed."));
    }

    [HttpGet("roles")]
    [Authorize]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _authService.GetRolesAsync();
        return Ok(ApiResponse<List<RoleDto>>.Ok(roles));
    }

    [HttpGet("permissions")]
    [Authorize]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await _authService.GetPermissionsAsync();
        return Ok(ApiResponse<List<PermissionDto>>.Ok(permissions));
    }
}
