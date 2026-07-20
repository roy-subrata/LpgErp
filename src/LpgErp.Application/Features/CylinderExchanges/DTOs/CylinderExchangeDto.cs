using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.CylinderExchanges.DTOs;

public class CylinderExchangeDto : IMapFrom<CylinderExchange>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid IncomingBrandId { get; set; }
    public string? IncomingBrandName { get; set; }
    public Guid IncomingCylinderSizeId { get; set; }
    public string? IncomingCylinderSizeName { get; set; }
    public int IncomingQuantity { get; set; }
    public Guid OutgoingBrandId { get; set; }
    public string? OutgoingBrandName { get; set; }
    public Guid OutgoingCylinderSizeId { get; set; }
    public string? OutgoingCylinderSizeName { get; set; }
    public int OutgoingQuantity { get; set; }
    public decimal ExchangeCharge { get; set; }
    public DateTime ExchangeDate { get; set; }
    public string? Notes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CylinderExchange, CylinderExchangeDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.IncomingBrandName, opt => opt.MapFrom(s => s.IncomingBrand.Name))
            .ForMember(d => d.IncomingCylinderSizeName, opt => opt.MapFrom(s => s.IncomingCylinderSize.Name))
            .ForMember(d => d.OutgoingBrandName, opt => opt.MapFrom(s => s.OutgoingBrand.Name))
            .ForMember(d => d.OutgoingCylinderSizeName, opt => opt.MapFrom(s => s.OutgoingCylinderSize.Name));
    }
}

public class CreateCylinderExchangeRequest
{
    public Guid CustomerId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid IncomingBrandId { get; set; }
    public Guid IncomingCylinderSizeId { get; set; }
    public int IncomingQuantity { get; set; }
    public Guid OutgoingBrandId { get; set; }
    public Guid OutgoingCylinderSizeId { get; set; }
    public int OutgoingQuantity { get; set; }
    public decimal ExchangeCharge { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCylinderExchangeRequest
{
    public Guid CustomerId { get; set; }
    public Guid IncomingBrandId { get; set; }
    public Guid IncomingCylinderSizeId { get; set; }
    public int IncomingQuantity { get; set; }
    public Guid OutgoingBrandId { get; set; }
    public Guid OutgoingCylinderSizeId { get; set; }
    public int OutgoingQuantity { get; set; }
    public decimal ExchangeCharge { get; set; }
    public string? Notes { get; set; }
}
