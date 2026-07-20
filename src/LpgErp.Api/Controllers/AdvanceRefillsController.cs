using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.AdvanceRefills;
using LpgErp.Application.Features.AdvanceRefills.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AdvanceRefillsController : ControllerBase
{
    private readonly IAdvanceRefillService _service;

    public AdvanceRefillsController(IAdvanceRefillService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdvanceRefillRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AdvanceRefillDto>.Fail(result.Error!));
        return Ok(ApiResponse<AdvanceRefillDto>.Ok(result.Data!));
    }

    [HttpGet("outstanding")]
    public async Task<IActionResult> GetOutstanding(CancellationToken cancellationToken)
    {
        var result = await _service.GetOutstandingAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<OutstandingCylinderDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<OutstandingCylinderDto>>.Ok(result.Data!));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByCustomerAsync(customerId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<AdvanceRefillDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<AdvanceRefillDto>>.Ok(result.Data!));
    }
}
