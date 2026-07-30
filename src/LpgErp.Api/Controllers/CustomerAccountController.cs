using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CustomerAccount;
using LpgErp.Application.Features.CustomerAccount.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

/// <summary>
/// One customer's account: what they owe, what they hold, and every line behind it.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class CustomerAccountController : ControllerBase
{
    private readonly ICustomerAccountService _service;

    public CustomerAccountController(ICustomerAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllSummariesAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<CustomerAccountSummaryDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<CustomerAccountSummaryDto>>.Ok(result.Data!));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetSummary(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetSummaryAsync(customerId, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<CustomerAccountSummaryDto>.Fail(result.Error!));

        return Ok(ApiResponse<CustomerAccountSummaryDto>.Ok(result.Data!));
    }

    [HttpGet("customer/{customerId:guid}/statement")]
    public async Task<IActionResult> GetStatement(
        Guid customerId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetStatementAsync(customerId, from, to, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<CustomerStatementDto>.Fail(result.Error!));

        return Ok(ApiResponse<CustomerStatementDto>.Ok(result.Data!));
    }

    [HttpGet("customer/{customerId:guid}/orders")]
    public async Task<IActionResult> GetOrders(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetOrdersAsync(customerId, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<IReadOnlyList<CustomerOrderDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<CustomerOrderDto>>.Ok(result.Data!));
    }

    [HttpGet("aging")]
    public async Task<IActionResult> GetAging(CancellationToken cancellationToken)
    {
        var result = await _service.GetAgingAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CustomerAgingDto>.Fail(result.Error!));

        return Ok(ApiResponse<CustomerAgingDto>.Ok(result.Data!));
    }
}
