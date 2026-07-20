using LpgErp.Application.Features.Salesmen;
using LpgErp.Application.Features.Salesmen.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class SalesmenController : BaseController<CreateSalesmanRequest, UpdateSalesmanRequest, SalesmanDto>
{
    public SalesmenController(ISalesmanService service) : base(service) { }
}
