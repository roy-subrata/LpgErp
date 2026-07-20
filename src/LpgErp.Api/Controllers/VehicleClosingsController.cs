using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.VehicleClosings;
using LpgErp.Application.Features.VehicleLoadings.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class VehicleClosingsController : ControllerBase
{
    private readonly IVehicleClosingService _service;

    public VehicleClosingsController(IVehicleClosingService service) { _service = service; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, ct);
        return Ok(ApiResponse<PagedResult<VehicleClosingDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<VehicleClosingDto>.Fail(result.Error!));
        return Ok(ApiResponse<VehicleClosingDto>.Ok(result.Data!));
    }
}
