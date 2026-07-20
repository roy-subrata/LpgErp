using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CustomerGasLedger;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomerGasLedgerController : ControllerBase
{
    private readonly ICustomerGasLedgerService _service;

    public CustomerGasLedgerController(ICustomerGasLedgerService service)
    {
        _service = service;
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerLedger(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetCustomerLedgerAsync(customerId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CustomerGasLedgerDto>.Fail(result.Error!));
        return Ok(ApiResponse<CustomerGasLedgerDto>.Ok(result.Data!));
    }
}
