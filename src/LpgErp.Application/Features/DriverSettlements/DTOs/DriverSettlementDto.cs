using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.DriverSettlements.DTOs;

public class DriverSettlementDto : IMapFrom<DriverSettlement>
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public string? DriverName { get; set; }
    public DateTime SettlementDate { get; set; }
    public Guid? VehicleLoadingId { get; set; }
    public int TripCount { get; set; }
    public decimal FuelCost { get; set; }
    public decimal Allowance { get; set; }
    public decimal LoadingCost { get; set; }
    public decimal UnloadingCost { get; set; }
    public decimal TripIncome { get; set; }
    public decimal CompanyPickupIncentive { get; set; }
    public decimal NetSettlement { get; set; }
    public decimal? Distance { get; set; }
    public string? Notes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<DriverSettlement, DriverSettlementDto>()
            .ForMember(d => d.DriverName, opt => opt.MapFrom(s => s.Driver.Name));
    }
}

public class CreateDriverSettlementRequest : IMapTo<DriverSettlement>
{
    public Guid DriverId { get; set; }
    public Guid? VehicleLoadingId { get; set; }
    public int TripCount { get; set; }
    public decimal FuelCost { get; set; }
    public decimal Allowance { get; set; }
    public decimal LoadingCost { get; set; }
    public decimal UnloadingCost { get; set; }
    public decimal TripIncome { get; set; }
    public decimal CompanyPickupIncentive { get; set; }
    public decimal? Distance { get; set; }
    public string? Notes { get; set; }
}

public class UpdateDriverSettlementRequest : IMapTo<DriverSettlement>
{
    public Guid DriverId { get; set; }
    public Guid? VehicleLoadingId { get; set; }
    public int TripCount { get; set; }
    public decimal FuelCost { get; set; }
    public decimal Allowance { get; set; }
    public decimal LoadingCost { get; set; }
    public decimal UnloadingCost { get; set; }
    public decimal TripIncome { get; set; }
    public decimal CompanyPickupIncentive { get; set; }
    public decimal? Distance { get; set; }
    public string? Notes { get; set; }
}
