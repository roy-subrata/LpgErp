using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CustomerNotifications;
using LpgErp.Application.Features.CustomerNotifications.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomerNotificationsController : ControllerBase
{
    private readonly ICustomerNotificationService _service;

    public CustomerNotificationsController(ICustomerNotificationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CustomerNotificationDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagedResult<CustomerNotificationDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByCustomerAsync(customerId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<CustomerNotificationDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CustomerNotificationDto>>.Ok(result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerNotificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<CustomerNotificationDto>.Fail(result.Error!));
        return Ok(ApiResponse<CustomerNotificationDto>.Ok(result.Data!));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<CustomerNotificationDto>.Fail(result.Error!));
        return Ok(ApiResponse<CustomerNotificationDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerNotificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<CustomerNotificationDto>.Fail(result.Error!));
        return Ok(ApiResponse<CustomerNotificationDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse.Ok("Deleted successfully."));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.MarkAsReadAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse.Ok("Notification marked as read."));
    }
}
