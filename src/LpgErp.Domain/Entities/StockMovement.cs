namespace LpgErp.Domain.Entities;

public enum StockMovementType
{
    PurchaseIn = 0,
    SaleOut = 1,
    TransferIn = 2,
    TransferOut = 3,
    Adjustment = 4,
    Return = 5
}

public class StockMovement : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public Guid? FromWarehouseId { get; set; }
    public Warehouse? FromWarehouse { get; set; }
    public Guid? ToWarehouseId { get; set; }
    public Warehouse? ToWarehouse { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public string? Reference { get; set; }
    public DateTime MovementDate { get; set; }
}
