using LpgErp.Application.Features.CylinderSizes;
using LpgErp.Application.Features.CylinderSizes.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class CylinderSizesController : BaseController<CreateCylinderSizeRequest, UpdateCylinderSizeRequest, CylinderSizeDto>
{
    public CylinderSizesController(ICylinderSizeService service) : base(service) { }
}
