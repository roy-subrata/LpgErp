namespace LpgErp.Domain.Entities;

public enum PaymentMethod
{
    Cash = 0,
    Credit = 1,
    Bank = 2,
    MobileBanking = 3
}

public enum PaymentDirection
{
    Inbound = 0,
    Outbound = 1
}

public class Payment : BaseEntity
{
    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
