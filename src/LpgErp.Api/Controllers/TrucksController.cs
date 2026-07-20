using LpgErp.Application.Features.Trucks;
using LpgErp.Application.Features.Trucks.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class TrucksController : BaseController<CreateTruckRequest, UpdateTruckRequest, TruckDto>
{
    public TrucksController(ITruckService service) : base(service) { }
}
