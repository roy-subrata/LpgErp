using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SuppliersController : BaseController<Supplier>
{
    public SuppliersController(IRepository<Supplier> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork) { }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Supplier supplier, CancellationToken cancellationToken)
    {
        await Repository.AddAsync(supplier, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, new { data = supplier });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Supplier supplier, CancellationToken cancellationToken)
    {
        var existing = await Repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        existing.Name = supplier.Name;
        existing.Code = supplier.Code;
        existing.ContactPerson = supplier.ContactPerson;
        existing.Phone = supplier.Phone;
        existing.Email = supplier.Email;
        existing.Address = supplier.Address;
        existing.IsLpgCompany = supplier.IsLpgCompany;
        existing.BrandId = supplier.BrandId;
        existing.IsActive = supplier.IsActive;

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
