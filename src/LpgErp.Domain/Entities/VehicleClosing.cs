namespace LpgErp.Domain.Entities;

public class VehicleClosing : BaseEntity
{
    public Guid VehicleLoadingId { get; set; }
    public VehicleLoading VehicleLoading { get; set; } = null!;
    public DateTime ClosingDate { get; set; }
    public decimal CashCollected { get; set; }
    public decimal CreditSales { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int CylinderExchanges { get; set; }
    public int ReturnedEmptyCylinders { get; set; }
    public int DamagedCount { get; set; }
    public int LeakageCount { get; set; }
    public int Variance { get; set; }
    public string? Notes { get; set; }

    public ICollection<VehicleClosingItem> Items { get; set; } = [];
}

public class VehicleClosingItem : BaseEntity
{
    public Guid VehicleClosingId { get; set; }
    public VehicleClosing VehicleClosing { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int LoadedQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public int DamagedQuantity { get; set; }
}
