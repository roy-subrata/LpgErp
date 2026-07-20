using LpgErp.Application.Features.Customers;
using LpgErp.Application.Features.Customers.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class CustomersController : BaseController<CreateCustomerRequest, UpdateCustomerRequest, CustomerDto>
{
    public CustomersController(ICustomerService service) : base(service) { }
}
