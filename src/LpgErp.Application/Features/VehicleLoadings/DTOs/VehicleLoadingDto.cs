using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.VehicleLoadings.DTOs;

public class VehicleLoadingDto : IMapFrom<VehicleLoading>
{
    public Guid Id { get; set; }
    public DateTime LoadingDate { get; set; }
    public Guid TruckId { get; set; }
    public string? TruckName { get; set; }
    public Guid DriverId { get; set; }
    public string? DriverName { get; set; }
    public Guid SalesmanId { get; set; }
    public string? SalesmanName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? RouteId { get; set; }
    public string? RouteName { get; set; }
    public VehicleLoadingStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<VehicleLoadingItemDto> Items { get; set; } = [];

    public void Mapping(Profile profile)
    {
        profile.CreateMap<VehicleLoading, VehicleLoadingDto>()
            .ForMember(d => d.TruckName, opt => opt.MapFrom(s => s.Truck.Name))
            .ForMember(d => d.DriverName, opt => opt.MapFrom(s => s.Driver.Name))
            .ForMember(d => d.SalesmanName, opt => opt.MapFrom(s => s.Salesman.Name))
            .ForMember(d => d.WarehouseName, opt => opt.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.RouteName, opt => opt.MapFrom(s => s.Route != null ? s.Route.Name : null));
    }
}

public class VehicleLoadingItemDto : IMapFrom<VehicleLoadingItem>
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int LoadedQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public int DamagedQuantity { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<VehicleLoadingItem, VehicleLoadingItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));
    }
}

public class CreateVehicleLoadingRequest
{
    public DateTime LoadingDate { get; set; }
    public Guid TruckId { get; set; }
    public Guid DriverId { get; set; }
    public Guid SalesmanId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? RouteId { get; set; }
    public string? Notes { get; set; }
    public List<CreateVehicleLoadingItemRequest> Items { get; set; } = [];
}

public class CreateVehicleLoadingItemRequest
{
    public Guid ProductId { get; set; }
    public int LoadedQuantity { get; set; }
}

public class UpdateVehicleLoadingRequest
{
    public DateTime LoadingDate { get; set; }
    public Guid TruckId { get; set; }
    public Guid DriverId { get; set; }
    public Guid SalesmanId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? RouteId { get; set; }
    public string? Notes { get; set; }
    public List<CreateVehicleLoadingItemRequest> Items { get; set; } = [];
}

public class VehicleClosingDto : IMapFrom<VehicleClosing>
{
    public Guid Id { get; set; }
    public Guid VehicleLoadingId { get; set; }
    public DateTime ClosingDate { get; set; }
    public decimal CashCollected { get; set; }
    public decimal CreditSales { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int CylinderExchanges { get; set; }
    public int ReturnedEmptyCylinders { get; set; }
    public int DamagedCount { get; set; }
    public int LeakageCount { get; set; }
    public int Variance { get; set; }
    public string? Notes { get; set; }
    public List<VehicleClosingItemDto> Items { get; set; } = [];

    public void Mapping(Profile profile)
    {
        profile.CreateMap<VehicleClosing, VehicleClosingDto>();
    }
}

public class VehicleClosingItemDto : IMapFrom<VehicleClosingItem>
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int LoadedQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public int DamagedQuantity { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<VehicleClosingItem, VehicleClosingItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));
    }
}

public class CreateVehicleClosingRequest
{
    public Guid VehicleLoadingId { get; set; }
    public decimal CashCollected { get; set; }
    public decimal CreditSales { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int CylinderExchanges { get; set; }
    public int ReturnedEmptyCylinders { get; set; }
    public int DamagedCount { get; set; }
    public int LeakageCount { get; set; }
    public int Variance { get; set; }
    public string? Notes { get; set; }
    public List<CreateVehicleClosingItemRequest> Items { get; set; } = [];
}

public class CreateVehicleClosingItemRequest
{
    public Guid ProductId { get; set; }
    public int LoadedQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public int DamagedQuantity { get; set; }
}
