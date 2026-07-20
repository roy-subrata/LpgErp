using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CylinderDeposits;
using LpgErp.Application.Features.CylinderDeposits.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CylinderDepositsController : ControllerBase
{
    private readonly ICylinderDepositService _service;

    public CylinderDepositsController(ICylinderDepositService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CylinderDepositDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagedResult<CylinderDepositDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCylinderDepositRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<CylinderDepositDto>.Fail(result.Error!));
        return Ok(ApiResponse<CylinderDepositDto>.Ok(result.Data!));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<CylinderDepositDto>.Fail(result.Error!));
        return Ok(ApiResponse<CylinderDepositDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCylinderDepositRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<CylinderDepositDto>.Fail(result.Error!));
        return Ok(ApiResponse<CylinderDepositDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse.Ok("Deleted successfully."));
    }
}
