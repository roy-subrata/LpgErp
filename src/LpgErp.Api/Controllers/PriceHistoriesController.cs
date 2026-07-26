using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.PriceHistories;
using LpgErp.Application.Features.PriceHistories.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PriceHistoriesController : ControllerBase
{
    private readonly IPriceHistoryService _service;

    public PriceHistoriesController(IPriceHistoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<PriceHistoryDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetAllAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<Result<IReadOnlyList<PriceHistoryDto>>>> GetByProduct(Guid productId)
    {
        var result = await _service.GetByProductAsync(productId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<PriceHistoryDto>>> Create([FromBody] CreatePriceHistoryRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (result.IsSuccess) return CreatedAtAction(nameof(GetByProduct), new { productId = request.ProductId }, result);
        return BadRequest(result);
    }
}
