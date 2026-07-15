using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController<T> : ControllerBase where T : BaseEntity
{
    protected readonly IRepository<T> Repository;
    protected readonly IUnitOfWork UnitOfWork;

    protected BaseController(IRepository<T> repository, IUnitOfWork unitOfWork)
    {
        Repository = repository;
        UnitOfWork = unitOfWork;
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await Repository.GetAllAsync(cancellationToken);
        return Ok(new { data = items });
    }

    [HttpGet("{id:guid}")]
    public virtual async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await Repository.GetByIdAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return Ok(new { data = item });
    }
}
