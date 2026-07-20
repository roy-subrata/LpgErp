using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.SalesmanSettlements.DTOs;

public class SalesmanSettlementDto : IMapFrom<SalesmanSettlement>
{
    public Guid Id { get; set; }
    public Guid SalesmanId { get; set; }
    public string? SalesmanName { get; set; }
    public DateTime SettlementDate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal Collection { get; set; }
    public decimal Commission { get; set; }
    public decimal DailyAllowance { get; set; }
    public decimal Bonus { get; set; }
    public decimal NetSettlement { get; set; }
    public string? Notes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SalesmanSettlement, SalesmanSettlementDto>()
            .ForMember(d => d.SalesmanName, opt => opt.MapFrom(s => s.Salesman.Name));
    }
}

public class CreateSalesmanSettlementRequest
{
    public Guid SalesmanId { get; set; }
    public decimal TotalSales { get; set; }
    public decimal Collection { get; set; }
    public decimal Commission { get; set; }
    public decimal DailyAllowance { get; set; }
    public decimal Bonus { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSalesmanSettlementRequest
{
    public Guid SalesmanId { get; set; }
    public decimal TotalSales { get; set; }
    public decimal Collection { get; set; }
    public decimal Commission { get; set; }
    public decimal DailyAllowance { get; set; }
    public decimal Bonus { get; set; }
    public string? Notes { get; set; }
}
