using LpgErp.Application.Features.Cylinders;
using LpgErp.Application.Features.Cylinders.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class CylindersController : BaseController<CreateCylinderRequest, UpdateCylinderRequest, CylinderDto>
{
    public CylindersController(ICylinderService service) : base(service) { }
}
