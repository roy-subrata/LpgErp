namespace LpgErp.Domain.Entities;

public class CylinderSize : BaseEntity
{
    public Guid BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal DepositAmount { get; set; }
    public bool IsActive { get; set; } = true;
}
