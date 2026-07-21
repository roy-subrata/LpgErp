using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.PurchaseOrders.DTOs;

public class PurchaseOrderDto : IMapFrom<PurchaseOrder>
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CommissionEarned { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? TransportCompanyId { get; set; }
    public string? TransportCompanyName { get; set; }
    public decimal TransportationCost { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = [];

    public void Mapping(Profile profile)
    {
        profile.CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.SupplierName, opt => opt.MapFrom(s => s.Supplier.Name))
            .ForMember(d => d.WarehouseName, opt => opt.MapFrom(s => s.Warehouse.Name))
            .ForMember(d => d.TransportCompanyName, opt => opt.MapFrom(s => s.TransportCompany != null ? s.TransportCompany.Name : null));
    }
}

public class PurchaseOrderItemDto : IMapFrom<PurchaseOrderItem>
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int OrderedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int DamagedQuantity { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));
    }
}

public class CreatePurchaseOrderRequest : IMapTo<PurchaseOrder>
{
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? TransportCompanyId { get; set; }
    public decimal TransportationCost { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = [];
}

public class CreatePurchaseOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int OrderedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdatePurchaseOrderRequest : IMapTo<PurchaseOrder>
{
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? TransportCompanyId { get; set; }
    public decimal TransportationCost { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = [];
}

public class ReceivePurchaseOrderRequest
{
    public List<ReceiveItemRequest> Items { get; set; } = [];
}

public class ReceiveItemRequest
{
    public Guid ProductId { get; set; }
    public int ReceivedQuantity { get; set; }
    public int DamagedQuantity { get; set; }
}
