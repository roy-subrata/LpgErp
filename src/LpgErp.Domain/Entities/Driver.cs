namespace LpgErp.Domain.Entities;

public class Driver : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
