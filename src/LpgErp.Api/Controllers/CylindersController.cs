using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CylindersController : BaseController<Cylinder>
{
    public CylindersController(IRepository<Cylinder> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork) { }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Cylinder cylinder, CancellationToken cancellationToken)
    {
        await Repository.AddAsync(cylinder, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cylinder.Id }, new { data = cylinder });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Cylinder cylinder, CancellationToken cancellationToken)
    {
        var existing = await Repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        existing.BrandId = cylinder.BrandId;
        existing.CylinderSizeId = cylinder.CylinderSizeId;
        existing.SerialNumber = cylinder.SerialNumber;
        existing.Status = cylinder.Status;
        existing.CurrentWarehouseId = cylinder.CurrentWarehouseId;
        existing.HasGas = cylinder.HasGas;

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
