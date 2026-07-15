namespace LpgErp.Domain.Entities;

public enum ProductType
{
    EmptyCylinder = 0,
    GasRefill = 1,
    NewPackage = 2,
    Accessory = 3
}

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public ProductType Type { get; set; }
    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public Guid? CylinderSizeId { get; set; }
    public CylinderSize? CylinderSize { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
}
