using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.TransportCompanies;
using LpgErp.Application.Features.TransportCompanies.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TransportCompaniesController : ControllerBase
{
    private readonly ITransportCompanyService _service;

    public TransportCompaniesController(ITransportCompanyService service) { _service = service; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize, ct);
        return Ok(ApiResponse<PagedResult<TransportCompanyDto>>.OkPaginated(result.Data!, result.Data!.Pagination));
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAllList(CancellationToken ct)
    {
        var result = await _service.GetAllListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TransportCompanyDto>>.Ok(result.Data!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<TransportCompanyDto>.Fail(result.Error!));
        return Ok(ApiResponse<TransportCompanyDto>.Ok(result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransportCompanyRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<TransportCompanyDto>.Fail(result.Error!));
        return Ok(ApiResponse<TransportCompanyDto>.Ok(result.Data!));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransportCompanyRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<TransportCompanyDto>.Fail(result.Error!));
        return Ok(ApiResponse<TransportCompanyDto>.Ok(result.Data!));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Fail(result.Error!));
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}
