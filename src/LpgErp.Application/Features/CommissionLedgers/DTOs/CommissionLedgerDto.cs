using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.CommissionLedgers.DTOs;

public class CommissionLedgerDto : IMapFrom<CommissionLedger>
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public string? PolicyName { get; set; }
    public CommissionEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string? EntityName { get; set; }
    public string PeriodKey { get; set; } = string.Empty;
    public int ActualQuantity { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal CommissionEarned { get; set; }
    public CommissionLedgerStatus Status { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime? AppliedDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CommissionLedger, CommissionLedgerDto>()
            .ForMember(d => d.PolicyName, opt => opt.MapFrom(s => s.Policy.Name));
    }
}

public class CalculateCommissionRequest
{
    public Guid PolicyId { get; set; }
    public string? PeriodKey { get; set; }
}

public class SettleCommissionRequest
{
    public Guid LedgerId { get; set; }
    public string? Reference { get; set; }
}
