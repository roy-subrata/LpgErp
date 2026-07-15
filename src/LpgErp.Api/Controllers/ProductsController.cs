using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : BaseController<Product>
{
    public ProductsController(IRepository<Product> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork) { }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product, CancellationToken cancellationToken)
    {
        await Repository.AddAsync(product, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { data = product });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Product product, CancellationToken cancellationToken)
    {
        var existing = await Repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        existing.Name = product.Name;
        existing.Code = product.Code;
        existing.Type = product.Type;
        existing.BrandId = product.BrandId;
        existing.CylinderSizeId = product.CylinderSizeId;
        existing.PurchasePrice = product.PurchasePrice;
        existing.SalePrice = product.SalePrice;
        existing.CurrentStock = product.CurrentStock;
        existing.MinimumStock = product.MinimumStock;
        existing.IsActive = product.IsActive;

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
