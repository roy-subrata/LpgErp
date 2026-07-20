namespace LpgErp.Domain.Entities;

public class CylinderExchange : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public Guid IncomingBrandId { get; set; }
    public Brand IncomingBrand { get; set; } = null!;
    public Guid IncomingCylinderSizeId { get; set; }
    public CylinderSize IncomingCylinderSize { get; set; } = null!;
    public int IncomingQuantity { get; set; }
    public Guid OutgoingBrandId { get; set; }
    public Brand OutgoingBrand { get; set; } = null!;
    public Guid OutgoingCylinderSizeId { get; set; }
    public CylinderSize OutgoingCylinderSize { get; set; } = null!;
    public int OutgoingQuantity { get; set; }
    public decimal ExchangeCharge { get; set; }
    public DateTime ExchangeDate { get; set; }
    public string? Notes { get; set; }
}
