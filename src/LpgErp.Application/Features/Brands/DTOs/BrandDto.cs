using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Brands.DTOs;

public class BrandDto : IMapFrom<Brand>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBrandRequest : IMapTo<Brand>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateBrandRequest : IMapTo<Brand>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}
