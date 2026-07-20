using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.CylinderSizes.DTOs;

public class CylinderSizeDto : IMapFrom<CylinderSize>
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal DepositAmount { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CylinderSize, CylinderSizeDto>()
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand.Name));
    }
}

public class CreateCylinderSizeRequest
{
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal DepositAmount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCylinderSizeRequest
{
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal DepositAmount { get; set; }
    public bool IsActive { get; set; } = true;
}
