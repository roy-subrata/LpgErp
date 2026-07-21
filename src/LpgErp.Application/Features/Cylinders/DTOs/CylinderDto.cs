using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Cylinders.DTOs;

public class CylinderDto : IMapFrom<Cylinder>
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid CylinderSizeId { get; set; }
    public string? CylinderSizeName { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public CylinderStatus Status { get; set; }
    public Guid? CurrentWarehouseId { get; set; }
    public string? CurrentWarehouseName { get; set; }
    public bool HasGas { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Cylinder, CylinderDto>()
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand.Name))
            .ForMember(d => d.CylinderSizeName, opt => opt.MapFrom(s => s.CylinderSize.Name))
            .ForMember(d => d.CurrentWarehouseName, opt => opt.MapFrom(s => s.CurrentWarehouse != null ? s.CurrentWarehouse.Name : null));
    }
}

public class CreateCylinderRequest : IMapTo<Cylinder>
{
    public Guid BrandId { get; set; }
    public Guid CylinderSizeId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public CylinderStatus Status { get; set; } = CylinderStatus.Available;
    public Guid? CurrentWarehouseId { get; set; }
    public bool HasGas { get; set; }
}

public class UpdateCylinderRequest : IMapTo<Cylinder>
{
    public Guid BrandId { get; set; }
    public Guid CylinderSizeId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public CylinderStatus Status { get; set; } = CylinderStatus.Available;
    public Guid? CurrentWarehouseId { get; set; }
    public bool HasGas { get; set; }
}
