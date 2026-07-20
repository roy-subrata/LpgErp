namespace LpgErp.Domain.Entities;

public class DriverSettlement : BaseEntity
{
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;
    public DateTime SettlementDate { get; set; }
    public Guid? VehicleLoadingId { get; set; }
    public VehicleLoading? VehicleLoading { get; set; }
    public int TripCount { get; set; }
    public decimal FuelCost { get; set; }
    public decimal Allowance { get; set; }
    public decimal LoadingCost { get; set; }
    public decimal UnloadingCost { get; set; }
    public decimal TripIncome { get; set; }
    public decimal CompanyPickupIncentive { get; set; }
    public decimal? Distance { get; set; }
    public decimal NetSettlement => TripIncome + CompanyPickupIncentive - FuelCost - Allowance - LoadingCost - UnloadingCost;
    public string? Notes { get; set; }
}
