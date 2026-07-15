namespace LpgErp.Domain.Entities;

public class Salesman : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public decimal DailyCommissionRate { get; set; }
    public decimal DailyAllowance { get; set; }
    public bool IsActive { get; set; } = true;
}
