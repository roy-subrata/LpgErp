using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.StockMovements.DTOs;

public class StockMovementDto : IMapFrom<StockMovement>
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public Guid? FromWarehouseId { get; set; }
    public string? FromWarehouseName { get; set; }
    public Guid? ToWarehouseId { get; set; }
    public string? ToWarehouseName { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public string? Reference { get; set; }
    public DateTime MovementDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<StockMovement, StockMovementDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
            .ForMember(d => d.FromWarehouseName, opt => opt.MapFrom(s => s.FromWarehouse != null ? s.FromWarehouse.Name : null))
            .ForMember(d => d.ToWarehouseName, opt => opt.MapFrom(s => s.ToWarehouse != null ? s.ToWarehouse.Name : null));
    }
}

public class CreateStockMovementRequest : IMapTo<StockMovement>
{
    public Guid ProductId { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public Guid? FromWarehouseId { get; set; }
    public Guid? ToWarehouseId { get; set; }
    public string? Reference { get; set; }
}
