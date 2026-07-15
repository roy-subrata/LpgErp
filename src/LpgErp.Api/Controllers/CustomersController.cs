using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController : BaseController<Customer>
{
    public CustomersController(IRepository<Customer> repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork) { }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Customer customer, CancellationToken cancellationToken)
    {
        await Repository.AddAsync(customer, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, new { data = customer });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Customer customer, CancellationToken cancellationToken)
    {
        var existing = await Repository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        existing.Name = customer.Name;
        existing.Code = customer.Code;
        existing.Type = customer.Type;
        existing.ContactPerson = customer.ContactPerson;
        existing.Phone = customer.Phone;
        existing.Email = customer.Email;
        existing.Address = customer.Address;
        existing.CreditLimit = customer.CreditLimit;
        existing.IsActive = customer.IsActive;

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
