using LpgErp.Application.Features.Suppliers;
using LpgErp.Application.Features.Suppliers.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class SuppliersController : BaseController<CreateSupplierRequest, UpdateSupplierRequest, SupplierDto>
{
    public SuppliersController(ISupplierService service) : base(service) { }
}
