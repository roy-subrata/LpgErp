using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BrandsController : BaseController<Brand>
{
    public BrandsController(IRepository<Brand> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork) { }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Brand brand, CancellationToken cancellationToken)
    {
        await Repository.AddAsync(brand, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = brand.Id }, new { data = brand });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Brand brand, CancellationToken cancellationToken)
    {
        var existing = await Repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        existing.Name = brand.Name;
        existing.Code = brand.Code;
        existing.IsActive = brand.IsActive;

        Repository.Update(existing);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { data = existing });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existing = await Repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        Repository.Delete(existing);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
