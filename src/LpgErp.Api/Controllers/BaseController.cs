using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public abstract class BaseController<TCreateRequest, TUpdateRequest, TResponseDto> : ControllerBase
    where TCreateRequest : class
    where TUpdateRequest : class
    where TResponseDto : class
{
    protected readonly IService<TCreateRequest, TUpdateRequest, TResponseDto> Service;

    protected BaseController(IService<TCreateRequest, TUpdateRequest, TResponseDto> service)
    {
        Service = service;
    }

    /// <summary>
    /// The permission-group prefix (e.g. "customers") this resource's view/create/edit/delete
    /// policies are named after. Null — the default — means this resource has no permission model
    /// defined yet, so every action here stays open to any authenticated user, exactly as before
    /// permission enforcement existed. Only override this where AppPermissions actually defines a
    /// matching group; naming one that doesn't exist would 403 every request.
    /// </summary>
    protected virtual string? PermissionGroup => null;

    /// <summary>
    /// Checks "{PermissionGroup}.{suffix}" via the same policy pipeline [Authorize(Policy=...)] uses.
    /// Imperative rather than a declarative attribute because the required policy name depends on
    /// which concrete controller is running — a decision attributes can't express per subclass.
    /// </summary>
    private async Task<IActionResult?> RequirePermissionAsync(string suffix, CancellationToken ct)
    {
        if (PermissionGroup is null) return null;

        var authorizationService = HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        var result = await authorizationService.AuthorizeAsync(User, $"{PermissionGroup}.{suffix}");
        return result.Succeeded ? null : Forbid();
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (await RequirePermissionAsync("view", cancellationToken) is IActionResult denied) return denied;

        var result = await Service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<TResponseDto>.Fail(result.Error!));

        return Ok(ApiResponse<PagedResult<TResponseDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpGet("list")]
    public virtual async Task<IActionResult> GetAllList(CancellationToken cancellationToken = default)
    {
        if (await RequirePermissionAsync("view", cancellationToken) is IActionResult denied) return denied;

        var result = await Service.GetAllListAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<TResponseDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<TResponseDto>>.Ok(result.Data!));
    }

    [HttpGet("{id:guid}")]
    public virtual async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (await RequirePermissionAsync("view", cancellationToken) is IActionResult denied) return denied;

        var result = await Service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<TResponseDto>.Fail(result.Error!));

        return Ok(ApiResponse<TResponseDto>.Ok(result.Data!));
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TCreateRequest request, CancellationToken cancellationToken)
    {
        if (await RequirePermissionAsync("create", cancellationToken) is IActionResult denied) return denied;

        // Request validation is applied globally by ValidationFilter.
        var result = await Service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<TResponseDto>.Fail(result.Error!));

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.GetType().GetProperty("Id")?.GetValue(result.Data) }, ApiResponse<TResponseDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    public virtual async Task<IActionResult> Update(Guid id, [FromBody] TUpdateRequest request, CancellationToken cancellationToken)
    {
        if (await RequirePermissionAsync("edit", cancellationToken) is IActionResult denied) return denied;

        var result = await Service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<TResponseDto>.Fail(result.Error!));

        return Ok(ApiResponse<TResponseDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    public virtual async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (await RequirePermissionAsync("delete", cancellationToken) is IActionResult denied) return denied;

        var result = await Service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse.Fail(result.Error!));

        return Ok(ApiResponse.Ok("Deleted successfully."));
    }
}
