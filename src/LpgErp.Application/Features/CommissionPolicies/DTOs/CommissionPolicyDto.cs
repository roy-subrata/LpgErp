using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.CommissionPolicies.DTOs;

public class CommissionPolicyDto : IMapFrom<CommissionPolicy>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommissionEntityType EntityType { get; set; }
    public string? EntityName { get; set; }
    public Guid EntityId { get; set; }
    public CommissionCalculationType CalculationType { get; set; }
    public CommissionPeriodType PeriodType { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? CylinderSizeId { get; set; }
    public string? CylinderSizeName { get; set; }
    public int TargetQuantity { get; set; }
    public decimal CommissionValue { get; set; }
    public string? TierConfig { get; set; }
    public bool AutoApply { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CommissionPolicy, CommissionPolicyDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product != null ? s.Product.Name : null))
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.CylinderSizeName, opt => opt.MapFrom(s => s.CylinderSize != null ? s.CylinderSize.Name : null));
    }
}

public class CreateCommissionPolicyRequest : IMapTo<CommissionPolicy>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommissionEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public CommissionCalculationType CalculationType { get; set; }
    public CommissionPeriodType PeriodType { get; set; } = CommissionPeriodType.Monthly;
    public Guid? ProductId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? CylinderSizeId { get; set; }
    public int TargetQuantity { get; set; }
    public decimal CommissionValue { get; set; }
    public string? TierConfig { get; set; }
    public bool AutoApply { get; set; } = true;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateCommissionPolicyRequest : IMapTo<CommissionPolicy>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommissionEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public CommissionCalculationType CalculationType { get; set; }
    public CommissionPeriodType PeriodType { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? CylinderSizeId { get; set; }
    public int TargetQuantity { get; set; }
    public decimal CommissionValue { get; set; }
    public string? TierConfig { get; set; }
    public bool AutoApply { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
