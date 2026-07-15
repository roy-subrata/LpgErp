namespace LpgErp.Domain.Entities;

public enum CylinderStatus
{
    Available = 0,
    WithCustomer = 1,
    InTransit = 2,
    Damaged = 3,
    UnderMaintenance = 4
}

public class Cylinder : BaseEntity
{
    public Guid BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    public Guid CylinderSizeId { get; set; }
    public CylinderSize CylinderSize { get; set; } = null!;
    public string SerialNumber { get; set; } = string.Empty;
    public CylinderStatus Status { get; set; } = CylinderStatus.Available;
    public Guid? CurrentWarehouseId { get; set; }
    public Warehouse? CurrentWarehouse { get; set; }
    public bool HasGas { get; set; }
}
