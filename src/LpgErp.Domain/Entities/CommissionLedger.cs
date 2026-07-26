namespace LpgErp.Domain.Entities;

public enum CommissionLedgerStatus
{
    Pending = 0,
    Earned = 1,
    Applied = 2,
    Expired = 3,
    Cancelled = 4
}

public class CommissionLedger : BaseEntity
{
    public Guid PolicyId { get; set; }
    public CommissionPolicy Policy { get; set; } = null!;
    public CommissionEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string PeriodKey { get; set; } = string.Empty;
    public int ActualQuantity { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal CommissionEarned { get; set; }
    public CommissionLedgerStatus Status { get; set; } = CommissionLedgerStatus.Pending;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime? AppliedDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
