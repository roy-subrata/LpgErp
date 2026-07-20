namespace LpgErp.Domain.Entities;

public enum CustomerType
{
    Retail = 0,
    WholesaleDealer = 1,
    Commercial = 2,
    Restaurant = 3,
    Hotel = 4,
    Industrial = 5
}

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public CustomerType Type { get; set; } = CustomerType.Retail;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentDueDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}
