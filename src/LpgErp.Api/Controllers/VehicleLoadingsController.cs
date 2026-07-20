using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.VehicleLoadings;
using LpgErp.Application.Features.VehicleLoadings.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class VehicleLoadingsController : ControllerBase
{
    private readonly IVehicleLoadingService _service;

    public VehicleLoadingsController(IVehicleLoadingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<VehicleLoadingDto>.Fail(result.Error!));
        return Ok(ApiResponse<PagedResult<VehicleLoadingDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse<VehicleLoadingDto>.Fail(result.Error!));
        return Ok(ApiResponse<VehicleLoadingDto>.Ok(result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleLoadingRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<VehicleLoadingDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, ApiResponse<VehicleLoadingDto>.Ok(result.Data!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleLoadingRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<VehicleLoadingDto>.Fail(result.Error!));
        return Ok(ApiResponse<VehicleLoadingDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(ApiResponse.Fail(result.Error!));
        return Ok(ApiResponse.Ok("Deleted successfully."));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CreateVehicleClosingRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CloseAsync(id, request, cancellationToken);
        if (!result.IsSuccess) return BadRequest(ApiResponse<VehicleClosingDto>.Fail(result.Error!));
        return Ok(ApiResponse<VehicleClosingDto>.Ok(result.Data!));
    }
}
