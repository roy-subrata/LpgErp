namespace LpgErp.Domain.Entities;

public enum CommissionEntityType
{
    Salesman = 0,
    Customer = 1,
    Supplier = 2,
    Driver = 3
}

public enum CommissionCalculationType
{
    Percentage = 0,
    FixedAmount = 1,
    PerUnit = 2,
    TargetBonus = 3,
    TieredPercentage = 4
}

public enum CommissionPeriodType
{
    OneTime = 0,
    Weekly = 1,
    Monthly = 2,
    Yearly = 3
}

public class CommissionPolicy : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CommissionEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public CommissionCalculationType CalculationType { get; set; }
    public CommissionPeriodType PeriodType { get; set; } = CommissionPeriodType.Monthly;
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public Guid? CylinderSizeId { get; set; }
    public CylinderSize? CylinderSize { get; set; }
    public int TargetQuantity { get; set; }
    public decimal CommissionValue { get; set; }
    public string? TierConfig { get; set; }
    public bool AutoApply { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CommissionTier
{
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public decimal CommissionPercent { get; set; }
}
