namespace LpgErp.Domain.Entities;

public enum SalesOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    Delivered = 2,
    Cancelled = 3
}

public class SalesOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal NetAmount => TotalAmount - Discount;
    public DateTime OrderDate { get; set; }
    public string? Notes { get; set; }
    public bool IsCreditSale { get; set; }

    public ICollection<SalesOrderItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
