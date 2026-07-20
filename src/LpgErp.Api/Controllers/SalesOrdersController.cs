using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.SalesOrders;
using LpgErp.Application.Features.SalesOrders.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _service;

    public SalesOrdersController(ISalesOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<SalesOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagedResult<SalesOrderDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<SalesOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<SalesOrderDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, ApiResponse<SalesOrderDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<SalesOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse.Ok("Deleted successfully."));
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ConfirmAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<SalesOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Data!));
    }

    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeliverAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<SalesOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<SalesOrderDto>.Ok(result.Data!));
    }
}
