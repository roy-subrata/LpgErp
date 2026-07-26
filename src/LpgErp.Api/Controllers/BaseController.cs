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

    [HttpGet]
    public virtual async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await Service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<TResponseDto>.Fail(result.Error!));

        return Ok(ApiResponse<PagedResult<TResponseDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpGet("list")]
    public virtual async Task<IActionResult> GetAllList(CancellationToken cancellationToken = default)
    {
        var result = await Service.GetAllListAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<TResponseDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<TResponseDto>>.Ok(result.Data!));
    }

    [HttpGet("{id:guid}")]
    public virtual async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<TResponseDto>.Fail(result.Error!));

        return Ok(ApiResponse<TResponseDto>.Ok(result.Data!));
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TCreateRequest request, CancellationToken cancellationToken)
    {
        // Request validation is applied globally by ValidationFilter.
        var result = await Service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<TResponseDto>.Fail(result.Error!));

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.GetType().GetProperty("Id")?.GetValue(result.Data) }, ApiResponse<TResponseDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    public virtual async Task<IActionResult> Update(Guid id, [FromBody] TUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await Service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<TResponseDto>.Fail(result.Error!));

        return Ok(ApiResponse<TResponseDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    public virtual async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse.Fail(result.Error!));

        return Ok(ApiResponse.Ok("Deleted successfully."));
    }
}
