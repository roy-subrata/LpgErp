using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.DailySalesSummaries.DTOs;

public class DailySalesSummaryDto : IMapFrom<DailySalesSummary>
{
    public Guid Id { get; set; }
    public DateTime SummaryDate { get; set; }
    public Guid VehicleLoadingId { get; set; }
    public Guid TruckId { get; set; }
    public string? TruckName { get; set; }
    public Guid DriverId { get; set; }
    public string? DriverName { get; set; }
    public Guid SalesmanId { get; set; }
    public string? SalesmanName { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }
    public decimal CreditSales { get; set; }
    public int PackagesSold { get; set; }
    public int RefillsSold { get; set; }
    public int EmptyCylindersSold { get; set; }
    public int AccessoriesSold { get; set; }
    public decimal PaymentsCollected { get; set; }
    public decimal DueCreated { get; set; }
    public int CylinderBalance { get; set; }
    public int OutstandingCylinders { get; set; }
    public int StockReturned { get; set; }
    public string? Notes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<DailySalesSummary, DailySalesSummaryDto>()
            .ForMember(d => d.TruckName, opt => opt.MapFrom(s => s.Truck.Name))
            .ForMember(d => d.DriverName, opt => opt.MapFrom(s => s.Driver.Name))
            .ForMember(d => d.SalesmanName, opt => opt.MapFrom(s => s.Salesman.Name));
    }
}

public class CreateDailySalesSummaryRequest : IMapTo<DailySalesSummary>
{
    public Guid VehicleLoadingId { get; set; }
    public Guid TruckId { get; set; }
    public Guid DriverId { get; set; }
    public Guid SalesmanId { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }
    public decimal CreditSales { get; set; }
    public int PackagesSold { get; set; }
    public int RefillsSold { get; set; }
    public int EmptyCylindersSold { get; set; }
    public int AccessoriesSold { get; set; }
    public decimal PaymentsCollected { get; set; }
    public decimal DueCreated { get; set; }
    public int CylinderBalance { get; set; }
    public int OutstandingCylinders { get; set; }
    public int StockReturned { get; set; }
    public string? Notes { get; set; }
}

public class UpdateDailySalesSummaryRequest : IMapTo<DailySalesSummary>
{
    public Guid VehicleLoadingId { get; set; }
    public Guid TruckId { get; set; }
    public Guid DriverId { get; set; }
    public Guid SalesmanId { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }
    public decimal CreditSales { get; set; }
    public int PackagesSold { get; set; }
    public int RefillsSold { get; set; }
    public int EmptyCylindersSold { get; set; }
    public int AccessoriesSold { get; set; }
    public decimal PaymentsCollected { get; set; }
    public decimal DueCreated { get; set; }
    public int CylinderBalance { get; set; }
    public int OutstandingCylinders { get; set; }
    public int StockReturned { get; set; }
    public string? Notes { get; set; }
}
