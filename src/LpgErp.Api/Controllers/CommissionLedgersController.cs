using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CommissionLedgers;
using LpgErp.Application.Features.CommissionLedgers.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CommissionLedgersController : ControllerBase
{
    private readonly ICommissionLedgerService _service;

    public CommissionLedgersController(ICommissionLedgerService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<CommissionLedgerDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetAllAsync(pageNumber, pageSize));
    }

    [HttpGet("entity/{entityType}/{entityId:guid}")]
    public async Task<ActionResult<Result<IReadOnlyList<CommissionLedgerDto>>>> GetByEntity(CommissionEntityType entityType, Guid entityId)
    {
        return Ok(await _service.GetByEntityAsync(entityType, entityId));
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<Result<CommissionLedgerDto>>> Calculate([FromBody] CalculateCommissionRequest request)
    {
        var result = await _service.CalculateAsync(request);
        if (result.IsSuccess) return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("settle")]
    public async Task<ActionResult<Result<CommissionLedgerDto>>> Settle([FromBody] SettleCommissionRequest request)
    {
        var result = await _service.SettleAsync(request);
        if (result.IsSuccess) return Ok(result);
        return BadRequest(result);
    }
}
