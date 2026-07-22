namespace LpgErp.Domain.Entities;

public class SalesOrderItem : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public int? CylinderExchangeQuantity { get; set; }
    /// <summary>For gas-refill lines: empties the customer handed over at delivery.
    /// Null = full swap (equals Quantity); 0 = advance refill (no empty given, cylinder owed).</summary>
    public int? EmptyReturnedQuantity { get; set; }
}
