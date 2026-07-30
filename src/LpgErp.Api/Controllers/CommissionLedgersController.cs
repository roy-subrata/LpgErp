using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CommissionLedgers;
using LpgErp.Application.Features.CommissionLedgers.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

// Previously had no [Authorize] at all — every action here was reachable without even logging in.
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class CommissionLedgersController : ControllerBase
{
    private readonly ICommissionLedgerService _service;

    public CommissionLedgersController(ICommissionLedgerService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = AppPermissions.Commission.View)]
    public async Task<ActionResult<Result<PagedResult<CommissionLedgerDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetAllAsync(pageNumber, pageSize));
    }

    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [Authorize(Policy = AppPermissions.Commission.View)]
    public async Task<ActionResult<Result<IReadOnlyList<CommissionLedgerDto>>>> GetByEntity(CommissionEntityType entityType, Guid entityId)
    {
        return Ok(await _service.GetByEntityAsync(entityType, entityId));
    }

    [HttpPost("calculate")]
    [Authorize(Policy = AppPermissions.Commission.Manage)]
    public async Task<ActionResult<Result<CommissionLedgerDto>>> Calculate([FromBody] CalculateCommissionRequest request)
    {
        var result = await _service.CalculateAsync(request);
        if (result.IsSuccess) return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("settle")]
    [Authorize(Policy = AppPermissions.Commission.Manage)]
    public async Task<ActionResult<Result<CommissionLedgerDto>>> Settle([FromBody] SettleCommissionRequest request)
    {
        var result = await _service.SettleAsync(request);
        if (result.IsSuccess) return Ok(result);
        return BadRequest(result);
    }
}
