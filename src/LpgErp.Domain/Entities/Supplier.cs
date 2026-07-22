namespace LpgErp.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsLpgCompany { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Commission granted by this supplier per cylinder received (e.g. 50 BDT/cylinder). 0 = no commission.</summary>
    public decimal CommissionPerCylinder { get; set; }

    /// <summary>Accumulated commission earned but not yet adjusted against a purchase. Reduced automatically on the next purchase order.</summary>
    public decimal CommissionBalance { get; set; }

    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }
}
