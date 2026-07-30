using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.SupplierAccount;
using LpgErp.Application.Features.SupplierAccount.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

/// <summary>
/// One supplier's account: what we owe them, what we've paid, and every line behind it.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class SupplierAccountController : ControllerBase
{
    private readonly ISupplierAccountService _service;

    public SupplierAccountController(ISupplierAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllSummariesAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<SupplierAccountSummaryDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<SupplierAccountSummaryDto>>.Ok(result.Data!));
    }

    [HttpGet("supplier/{supplierId:guid}")]
    public async Task<IActionResult> GetSummary(Guid supplierId, CancellationToken cancellationToken)
    {
        var result = await _service.GetSummaryAsync(supplierId, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<SupplierAccountSummaryDto>.Fail(result.Error!));

        return Ok(ApiResponse<SupplierAccountSummaryDto>.Ok(result.Data!));
    }

    [HttpGet("supplier/{supplierId:guid}/statement")]
    public async Task<IActionResult> GetStatement(
        Guid supplierId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetStatementAsync(supplierId, from, to, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<SupplierStatementDto>.Fail(result.Error!));

        return Ok(ApiResponse<SupplierStatementDto>.Ok(result.Data!));
    }

    [HttpGet("supplier/{supplierId:guid}/orders")]
    public async Task<IActionResult> GetOrders(Guid supplierId, CancellationToken cancellationToken)
    {
        var result = await _service.GetOrdersAsync(supplierId, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<IReadOnlyList<SupplierOrderDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<SupplierOrderDto>>.Ok(result.Data!));
    }
}
