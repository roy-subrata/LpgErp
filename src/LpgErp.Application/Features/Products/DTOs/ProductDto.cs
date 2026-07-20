using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Products.DTOs;

public class ProductDto : IMapFrom<Product>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public ProductType Type { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? CylinderSizeId { get; set; }
    public string? CylinderSizeName { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Product, ProductDto>()
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.CylinderSizeName, opt => opt.MapFrom(s => s.CylinderSize != null ? s.CylinderSize.Name : null));
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public ProductType Type { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? CylinderSizeId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public ProductType Type { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? CylinderSizeId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
}
