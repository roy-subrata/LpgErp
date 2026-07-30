using AutoMapper;
using LpgErp.Application.Common.Mappings;
using LpgErp.Application.Features.Payments.DTOs;
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
    public decimal CommissionApplied { get; set; }
    public decimal LeakageCredit { get; set; }
    public decimal NetPayable => TotalAmount + TransportationCost - CommissionApplied - LeakageCredit;
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? TransportCompanyId { get; set; }
    public string? TransportCompanyName { get; set; }
    public decimal TransportationCost { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = [];
    public List<PurchaseOrderLeakageDto> Leakages { get; set; } = [];

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
    public int MissingQuantity { get; set; }
    public int ShortQuantity { get; set; }
    public int? EmptyReturnedQuantity { get; set; }
    public int EmptySentQuantity { get; set; }
    public int EmptyOwedQuantity { get; set; }
    public int ProductType { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(d => d.ProductType, opt => opt.MapFrom(s => (int)s.Product.Type))
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));
    }
}

public class PurchaseOrderLeakageDto : IMapFrom<PurchaseOrderLeakage>
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid CylinderSizeId { get; set; }
    public string? CylinderSizeName { get; set; }
    public int Quantity { get; set; }
    public LeakageResolution Resolution { get; set; }
    public decimal CreditAmount { get; set; }
    public int SettledQuantity { get; set; }
    public string? Notes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<PurchaseOrderLeakage, PurchaseOrderLeakageDto>()
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand.Name))
            .ForMember(d => d.CylinderSizeName, opt => opt.MapFrom(s => s.CylinderSize.Name));
    }
}

/// <summary>Leaking cylinders being sent back to the company with an order.</summary>
public class CreateLeakageRequest
{
    public Guid BrandId { get; set; }
    public Guid CylinderSizeId { get; set; }
    public int Quantity { get; set; }
    public LeakageResolution Resolution { get; set; }

    /// <summary>Only meaningful for a credit adjustment; ignored for free refills and replacements.</summary>
    public decimal CreditAmount { get; set; }

    public string? Notes { get; set; }
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

    /// <summary>Leaking cylinders going back to the company with this order.</summary>
    public List<CreateLeakageRequest> Leakages { get; set; } = [];

    /// <summary>How the supplier was paid, if anything was paid up front. Null when nothing was paid.</summary>
    public OrderPaymentRequest? Payment { get; set; }
}

public class CreatePurchaseOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int OrderedQuantity { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Empties promised to the company for this refill line. Null keeps the normal one-for-one
    /// swap; a smaller number means cylinders will be owed.
    /// </summary>
    public int? EmptyReturnedQuantity { get; set; }
}

public class UpdatePurchaseOrderRequest : IMapTo<PurchaseOrder>
{
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? TransportCompanyId { get; set; }
    public decimal TransportationCost { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = [];
    public List<CreateLeakageRequest> Leakages { get; set; } = [];
}

public class ReceivePurchaseOrderRequest
{
    public List<ReceiveItemRequest> Items { get; set; } = [];

    /// <summary>Leaking cylinders settled by the company in this delivery, keyed by leakage line.</summary>
    public List<SettleLeakageRequest> Leakages { get; set; } = [];
}

public class ReceiveItemRequest
{
    public Guid ProductId { get; set; }
    public int ReceivedQuantity { get; set; }
    public int DamagedQuantity { get; set; }
    public int MissingQuantity { get; set; }

    /// <summary>
    /// Empty cylinders handed to the company with this delivery. Null means the normal one-for-one
    /// swap against what was received.
    /// </summary>
    public int? EmptySentQuantity { get; set; }
}

public class SettleLeakageRequest
{
    public Guid LeakageId { get; set; }

    /// <summary>Leaking cylinders the company took and settled in this delivery.</summary>
    public int SettledQuantity { get; set; }
}
