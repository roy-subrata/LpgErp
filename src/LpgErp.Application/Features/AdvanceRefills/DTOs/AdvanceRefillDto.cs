using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.AdvanceRefills.DTOs;

public class AdvanceRefillDto : IMapFrom<CustomerCylinderBalance>
{
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? BrandName { get; set; }
    public string? CylinderSizeName { get; set; }
    public int Received { get; set; }
    public int Returned { get; set; }
    public int Outstanding { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CustomerCylinderBalance, AdvanceRefillDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand.Name))
            .ForMember(d => d.CylinderSizeName, opt => opt.MapFrom(s => s.CylinderSize.Name));
    }
}

public class CreateAdvanceRefillRequest
{
    public Guid CustomerId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class OutstandingCylinderDto : IMapFrom<CustomerCylinderBalance>
{
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid CylinderSizeId { get; set; }
    public string? CylinderSizeName { get; set; }
    public int Outstanding { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CustomerCylinderBalance, OutstandingCylinderDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand.Name))
            .ForMember(d => d.CylinderSizeName, opt => opt.MapFrom(s => s.CylinderSize.Name));
    }
}
