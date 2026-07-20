namespace LpgErp.Domain.Entities;

public enum PurchaseOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    InTransit = 2,
    PartiallyReceived = 3,
    Received = 4,
    Cancelled = 5
}

public class PurchaseOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public decimal CommissionEarned { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }
    public Guid? TransportCompanyId { get; set; }
    public TransportCompany? TransportCompany { get; set; }
    public decimal TransportationCost { get; set; }
    public DateTime? DueDate { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
