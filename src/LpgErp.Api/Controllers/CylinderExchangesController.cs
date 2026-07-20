using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CylinderExchanges;
using LpgErp.Application.Features.CylinderExchanges.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CylinderExchangesController : ControllerBase
{
    private readonly ICylinderExchangeService _service;

    public CylinderExchangesController(ICylinderExchangeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CylinderExchangeDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagedResult<CylinderExchangeDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCylinderExchangeRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<CylinderExchangeDto>.Fail(result.Error!));
        return Ok(ApiResponse<CylinderExchangeDto>.Ok(result.Data!));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<CylinderExchangeDto>.Fail(result.Error!));
        return Ok(ApiResponse<CylinderExchangeDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCylinderExchangeRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<CylinderExchangeDto>.Fail(result.Error!));
        return Ok(ApiResponse<CylinderExchangeDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse.Ok("Deleted successfully."));
    }
}
