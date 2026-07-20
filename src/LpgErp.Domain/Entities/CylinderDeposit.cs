namespace LpgErp.Domain.Entities;

public enum CylinderDepositType
{
    Paid = 0,
    Returned = 1,
    Refund = 2
}

public class CylinderDeposit : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid CylinderSizeId { get; set; }
    public CylinderSize CylinderSize { get; set; } = null!;
    public CylinderDepositType Type { get; set; }
    public decimal Amount { get; set; }
    public int Quantity { get; set; }
    public DateTime DepositDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
