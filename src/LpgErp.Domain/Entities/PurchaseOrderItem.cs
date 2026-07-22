namespace LpgErp.Domain.Entities;

public class PurchaseOrderItem : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int OrderedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => OrderedQuantity * UnitPrice;
    public int DamagedQuantity { get; set; }
    /// <summary>Units confirmed missing/lost in transit during receiving (distinct from damaged units that arrived).</summary>
    public int MissingQuantity { get; set; }
    /// <summary>Ordered units not yet received — the outstanding short-delivery balance.</summary>
    public int ShortQuantity => OrderedQuantity - ReceivedQuantity;
}
