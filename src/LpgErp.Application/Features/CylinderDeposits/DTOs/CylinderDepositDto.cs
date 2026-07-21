using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.CylinderDeposits.DTOs;

public class CylinderDepositDto : IMapFrom<CylinderDeposit>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid CylinderSizeId { get; set; }
    public string? CylinderSizeName { get; set; }
    public CylinderDepositType Type { get; set; }
    public decimal Amount { get; set; }
    public int Quantity { get; set; }
    public DateTime DepositDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CylinderDeposit, CylinderDepositDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.CylinderSizeName, opt => opt.MapFrom(s => s.CylinderSize.Name));
    }
}

public class CreateCylinderDepositRequest : IMapTo<CylinderDeposit>
{
    public Guid CustomerId { get; set; }
    public Guid CylinderSizeId { get; set; }
    public CylinderDepositType Type { get; set; }
    public decimal Amount { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCylinderDepositRequest : IMapTo<CylinderDeposit>
{
    public Guid CustomerId { get; set; }
    public Guid CylinderSizeId { get; set; }
    public CylinderDepositType Type { get; set; }
    public decimal Amount { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
