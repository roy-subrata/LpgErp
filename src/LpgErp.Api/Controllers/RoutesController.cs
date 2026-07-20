using LpgErp.Application.Features.Routes;
using LpgErp.Application.Features.Routes.DTOs;

namespace LpgErp.Api.Controllers;

public class RoutesController : BaseController<CreateRouteRequest, UpdateRouteRequest, RouteDto>
{
    public RoutesController(IRouteService service) : base(service) { }
}
