using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CustomerCylinderLedger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class CustomerCylinderLedgerController : ControllerBase
{
    private readonly ICustomerCylinderLedgerService _service;

    public CustomerCylinderLedgerController(ICustomerCylinderLedgerService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllPagedAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CustomerCylinderBalanceDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagedResult<CustomerCylinderBalanceDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByCustomerAsync(customerId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<CustomerCylinderBalanceDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CustomerCylinderBalanceDto>>.Ok(result.Data!));
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustCylinderBalanceRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.AdjustBalanceAsync(request, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CustomerCylinderBalanceDto>.Fail(result.Error!));
        return Ok(ApiResponse<CustomerCylinderBalanceDto>.Ok(result.Data!));
    }

    [HttpPost("forfeit")]
    public async Task<IActionResult> Forfeit([FromBody] ForfeitCylinderRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.ForfeitAsync(request, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ForfeitCylinderResultDto>.Fail(result.Error!));
        return Ok(ApiResponse<ForfeitCylinderResultDto>.Ok(result.Data!));
    }
}
