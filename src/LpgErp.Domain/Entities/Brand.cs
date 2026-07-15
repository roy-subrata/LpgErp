namespace LpgErp.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CylinderSize> CylinderSizes { get; set; } = [];
    public ICollection<Supplier> Suppliers { get; set; } = [];
}
