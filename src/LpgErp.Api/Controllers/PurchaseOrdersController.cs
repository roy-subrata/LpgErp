using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.PurchaseOrders;
using LpgErp.Application.Features.PurchaseOrders.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = AppPermissions.PurchaseOrders.View)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PurchaseOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagedResult<PurchaseOrderDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPermissions.PurchaseOrders.View)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<PurchaseOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result.Data!));
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.PurchaseOrders.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<PurchaseOrderDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, ApiResponse<PurchaseOrderDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPermissions.PurchaseOrders.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<PurchaseOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPermissions.PurchaseOrders.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse.Ok("Deleted successfully."));
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = AppPermissions.PurchaseOrders.Edit)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ConfirmAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<PurchaseOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result.Data!));
    }

    [HttpPost("{id:guid}/receive")]
    [Authorize(Policy = AppPermissions.PurchaseOrders.Receive)]
    public async Task<IActionResult> Receive(Guid id, [FromBody] ReceivePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.ReceiveAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<PurchaseOrderDto>.Fail(result.Error!));
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result.Data!));
    }
}
