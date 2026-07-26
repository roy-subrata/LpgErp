using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CommissionPolicies;
using LpgErp.Application.Features.CommissionPolicies.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CommissionPoliciesController : ControllerBase
{
    private readonly ICommissionPolicyService _service;

    public CommissionPoliciesController(ICommissionPolicyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<CommissionPolicyDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetAllAsync(pageNumber, pageSize));
    }

    [HttpGet("entity/{entityType}/{entityId:guid}")]
    public async Task<ActionResult<Result<IReadOnlyList<CommissionPolicyDto>>>> GetByEntity(CommissionEntityType entityType, Guid entityId)
    {
        return Ok(await _service.GetByEntityAsync(entityType, entityId));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<CommissionPolicyDto>>> GetById(Guid id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<Result<CommissionPolicyDto>>> Create([FromBody] CreateCommissionPolicyRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (result.IsSuccess) return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        return BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<CommissionPolicyDto>>> Update(Guid id, [FromBody] UpdateCommissionPolicyRequest request)
    {
        return Ok(await _service.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result>> Delete(Guid id)
    {
        return Ok(await _service.DeleteAsync(id));
    }
}
