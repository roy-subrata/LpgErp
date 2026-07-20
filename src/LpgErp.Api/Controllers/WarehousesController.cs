using LpgErp.Application.Features.Warehouses;
using LpgErp.Application.Features.Warehouses.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class WarehousesController : BaseController<CreateWarehouseRequest, UpdateWarehouseRequest, WarehouseDto>
{
    public WarehousesController(IWarehouseService service) : base(service) { }
}
