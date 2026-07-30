namespace LpgErp.Domain.Entities;

public class StockLevel : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>Good stock, available to sell or send for refilling.</summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Leaking or otherwise unusable cylinders held at this warehouse, waiting to go back to the
    /// company. Counted apart from <see cref="Quantity"/> because they cannot be sold or refilled.
    /// </summary>
    public int DamagedQuantity { get; set; }
}
