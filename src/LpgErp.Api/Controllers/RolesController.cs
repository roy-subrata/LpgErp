using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Roles;
using LpgErp.Application.Features.Roles.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

/// <summary>
/// Role definitions — what a role is called and which permissions it grants. Assigning an
/// existing role to a user is a separate, already-built concern under AuthController; this is
/// what lets an admin change what a role means in the first place.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<RoleSummaryDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<RoleSummaryDto>>.Ok(result.Data!));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<RoleSummaryDto>.Fail(result.Error!));
        return Ok(ApiResponse<RoleSummaryDto>.Ok(result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<RoleSummaryDto>.Fail(result.Error!));
        return Ok(ApiResponse<RoleSummaryDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<RoleSummaryDto>.Fail(result.Error!));
        return Ok(ApiResponse<RoleSummaryDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse.Ok("Role deleted."));
    }
}
