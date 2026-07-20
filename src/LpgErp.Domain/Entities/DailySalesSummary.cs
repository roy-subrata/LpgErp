namespace LpgErp.Domain.Entities;

public class DailySalesSummary : BaseEntity
{
    public DateTime SummaryDate { get; set; }
    public Guid VehicleLoadingId { get; set; }
    public VehicleLoading VehicleLoading { get; set; } = null!;
    public Guid TruckId { get; set; }
    public Truck Truck { get; set; } = null!;
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;
    public Guid SalesmanId { get; set; }
    public Salesman Salesman { get; set; } = null!;
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }
    public decimal CreditSales { get; set; }
    public int PackagesSold { get; set; }
    public int RefillsSold { get; set; }
    public int EmptyCylindersSold { get; set; }
    public int AccessoriesSold { get; set; }
    public decimal PaymentsCollected { get; set; }
    public decimal DueCreated { get; set; }
    public int CylinderBalance { get; set; }
    public int OutstandingCylinders { get; set; }
    public int StockReturned { get; set; }
    public string? Notes { get; set; }
}
