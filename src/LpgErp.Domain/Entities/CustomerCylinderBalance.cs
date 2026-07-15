namespace LpgErp.Domain.Entities;

public class CustomerCylinderBalance : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    public Guid CylinderSizeId { get; set; }
    public CylinderSize CylinderSize { get; set; } = null!;
    public int Received { get; set; }
    public int Returned { get; set; }
    public int Outstanding => Received - Returned;
}
