using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Salesmen.DTOs;

public class SalesmanDto : IMapFrom<Salesman>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public decimal DailyCommissionRate { get; set; }
    public decimal DailyAllowance { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSalesmanRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public decimal DailyCommissionRate { get; set; }
    public decimal DailyAllowance { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSalesmanRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public decimal DailyCommissionRate { get; set; }
    public decimal DailyAllowance { get; set; }
    public bool IsActive { get; set; } = true;
}
