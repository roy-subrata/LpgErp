using LpgErp.Application.Features.Products;
using LpgErp.Application.Features.Products.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LpgErp.Api.Controllers;

public class ProductsController : BaseController<CreateProductRequest, UpdateProductRequest, ProductDto>
{
    public ProductsController(IProductService service) : base(service) { }
}
