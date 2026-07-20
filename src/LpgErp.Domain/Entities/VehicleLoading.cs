namespace LpgErp.Domain.Entities;

public enum VehicleLoadingStatus
{
    Dispatched = 0,
    InTransit = 1,
    Returned = 2
}

public class VehicleLoading : BaseEntity
{
    public DateTime LoadingDate { get; set; }
    public Guid TruckId { get; set; }
    public Truck Truck { get; set; } = null!;
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;
    public Guid SalesmanId { get; set; }
    public Salesman Salesman { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Guid? RouteId { get; set; }
    public Route? Route { get; set; }
    public VehicleLoadingStatus Status { get; set; } = VehicleLoadingStatus.Dispatched;
    public string? Notes { get; set; }

    public ICollection<VehicleLoadingItem> Items { get; set; } = [];
}

public class VehicleLoadingItem : BaseEntity
{
    public Guid VehicleLoadingId { get; set; }
    public VehicleLoading VehicleLoading { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int LoadedQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public int DamagedQuantity { get; set; }
    public int? Price { get; set; }
}
