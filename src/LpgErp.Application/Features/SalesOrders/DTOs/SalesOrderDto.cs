using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.SalesOrders.DTOs;

public class SalesOrderDto : IMapFrom<SalesOrder>
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public SalesOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Notes { get; set; }
    public bool IsCreditSale { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? TransportCompanyId { get; set; }
    public string? TransportCompanyName { get; set; }
    public Guid? RouteId { get; set; }
    public string? RouteName { get; set; }
    public List<SalesOrderItemDto> Items { get; set; } = [];

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SalesOrder, SalesOrderDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.WarehouseName, opt => opt.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.TransportCompanyName, opt => opt.MapFrom(s => s.TransportCompany != null ? s.TransportCompany.Name : null))
            .ForMember(d => d.RouteName, opt => opt.MapFrom(s => s.Route != null ? s.Route.Name : null));
    }
}

public class SalesOrderItemDto : IMapFrom<SalesOrderItem>
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int? CylinderExchangeQuantity { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SalesOrderItem, SalesOrderItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));
    }
}

public class CreateSalesOrderRequest
{
    public Guid CustomerId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Discount { get; set; }
    public string? Notes { get; set; }
    public bool IsCreditSale { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? TransportCompanyId { get; set; }
    public Guid? RouteId { get; set; }
    public List<CreateSalesOrderItemRequest> Items { get; set; } = [];
}

public class CreateSalesOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int? CylinderExchangeQuantity { get; set; }
}

public class UpdateSalesOrderRequest
{
    public Guid CustomerId { get; set; }
    public Guid WarehouseId { get; set; }
    public SalesOrderStatus Status { get; set; }
    public decimal Discount { get; set; }
    public string? Notes { get; set; }
    public bool IsCreditSale { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? TransportCompanyId { get; set; }
    public Guid? RouteId { get; set; }
    public List<CreateSalesOrderItemRequest> Items { get; set; } = [];
}
