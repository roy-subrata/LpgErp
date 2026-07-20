namespace LpgErp.Domain.Entities;

public class SalesmanSettlement : BaseEntity
{
    public Guid SalesmanId { get; set; }
    public Salesman Salesman { get; set; } = null!;
    public DateTime SettlementDate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal Collection { get; set; }
    public decimal Commission { get; set; }
    public decimal DailyAllowance { get; set; }
    public decimal Bonus { get; set; }
    public decimal NetSettlement => Collection + Commission + DailyAllowance + Bonus;
    public string? Notes { get; set; }
}
