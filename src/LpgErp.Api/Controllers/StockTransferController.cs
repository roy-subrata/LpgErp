using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.StockTransfer;
using LpgErp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class StockTransferController : ControllerBase
{
    private readonly IStockTransferService _service;

    public StockTransferController(IStockTransferService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.Inventory.Transfer)]
    public async Task<IActionResult> Transfer([FromBody] StockTransferRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.TransferAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<StockTransferResponse>.Fail(result.Error!));
        return Ok(ApiResponse<StockTransferResponse>.Ok(result.Data!));
    }

    [HttpGet("history")]
    [Authorize(Policy = AppPermissions.Inventory.View)]
    public async Task<IActionResult> GetHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetHistoryAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<StockMovementDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagedResult<StockMovementDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }
}
