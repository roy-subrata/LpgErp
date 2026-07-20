using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.SalesmanSettlements;
using LpgErp.Application.Features.SalesmanSettlements.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SalesmanSettlementsController : ControllerBase
{
    private readonly ISalesmanSettlementService _service;

    public SalesmanSettlementsController(ISalesmanSettlementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<SalesmanSettlementDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagedResult<SalesmanSettlementDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesmanSettlementRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<SalesmanSettlementDto>.Fail(result.Error!));
        return Ok(ApiResponse<SalesmanSettlementDto>.Ok(result.Data!));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<SalesmanSettlementDto>.Fail(result.Error!));
        return Ok(ApiResponse<SalesmanSettlementDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesmanSettlementRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<SalesmanSettlementDto>.Fail(result.Error!));
        return Ok(ApiResponse<SalesmanSettlementDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse.Ok("Deleted successfully."));
    }
}
