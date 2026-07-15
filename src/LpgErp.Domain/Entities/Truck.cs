namespace LpgErp.Domain.Entities;

public class Truck : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
