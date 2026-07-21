using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Customers.DTOs;

public class CustomerDto : IMapFrom<Customer>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public CustomerType Type { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public int PaymentDueDays { get; set; } = 30;
}

public class CreateCustomerRequest : IMapTo<Customer>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public CustomerType Type { get; set; } = CustomerType.Retail;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public int PaymentDueDays { get; set; } = 30;
}

public class UpdateCustomerRequest : IMapTo<Customer>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public CustomerType Type { get; set; } = CustomerType.Retail;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public int PaymentDueDays { get; set; } = 30;
}
