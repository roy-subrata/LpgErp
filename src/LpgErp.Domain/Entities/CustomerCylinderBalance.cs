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

    /// <summary>
    /// Cylinders the customer kept permanently and paid for instead of returning — settled, but
    /// distinct from a physical return, so reporting can still tell the two apart.
    /// </summary>
    public int Forfeited { get; set; }

    public int Outstanding => Received - Returned - Forfeited;
}
