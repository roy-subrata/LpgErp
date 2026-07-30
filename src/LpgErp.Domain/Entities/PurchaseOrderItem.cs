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

    /// <summary>
    /// For gas-refill lines: empty cylinders sent to the company against this line. The company
    /// refills one cylinder for one empty, so null means the normal swap (equals OrderedQuantity).
    /// A smaller number means cylinders are owed to the company.
    /// </summary>
    public int? EmptyReturnedQuantity { get; set; }

    /// <summary>Empties actually handed over so far, tracked as the order is received in parts.</summary>
    public int EmptySentQuantity { get; set; }

    /// <summary>Empties still to send for this line — the shortfall owed to the company.</summary>
    public int EmptyOwedQuantity => (EmptyReturnedQuantity ?? OrderedQuantity) - EmptySentQuantity;
    /// <summary>Ordered units not yet received — the outstanding short-delivery balance.</summary>
    public int ShortQuantity => OrderedQuantity - ReceivedQuantity;
}
