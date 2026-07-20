using LpgErp.Application.Features.Brands;
using LpgErp.Application.Features.Brands.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class BrandsController : BaseController<CreateBrandRequest, UpdateBrandRequest, BrandDto>
{
    public BrandsController(IBrandService service) : base(service) { }
}
