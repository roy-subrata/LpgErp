using LpgErp.Application.Features.Drivers;
using LpgErp.Application.Features.Drivers.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class DriversController : BaseController<CreateDriverRequest, UpdateDriverRequest, DriverDto>
{
    public DriversController(IDriverService service) : base(service) { }
}
