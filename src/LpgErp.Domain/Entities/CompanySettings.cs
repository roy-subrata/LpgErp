namespace LpgErp.Domain.Entities;

/// <summary>
/// The distributor's own business profile — shop name, address, contact details. A singleton:
/// exactly one row exists, created on first read if missing, and only ever updated, never created
/// or deleted through the API.
/// </summary>
public class CompanySettings : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
}
