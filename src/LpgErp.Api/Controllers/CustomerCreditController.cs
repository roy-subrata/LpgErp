using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CustomerCredit;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomerCreditController : ControllerBase
{
    private readonly ICustomerCreditService _service;

    public CustomerCreditController(ICustomerCreditService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllCreditSummariesAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<CustomerCreditSummaryDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CustomerCreditSummaryDto>>.Ok(result.Data!));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetCustomerCreditSummaryAsync(customerId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CustomerCreditSummaryDto>.Fail(result.Error!));
        return Ok(ApiResponse<CustomerCreditSummaryDto>.Ok(result.Data!));
    }

    [HttpGet("aging")]
    public async Task<IActionResult> GetAging(CancellationToken cancellationToken)
    {
        var result = await _service.GetCreditAgingAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CreditAgingReportDto>.Fail(result.Error!));
        return Ok(ApiResponse<CreditAgingReportDto>.Ok(result.Data!));
    }
}
