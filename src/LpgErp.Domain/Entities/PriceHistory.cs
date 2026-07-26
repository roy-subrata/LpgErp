namespace LpgErp.Domain.Entities;

public enum PriceType
{
    Purchase = 0,
    Sale = 1
}

public enum PriceChangeReason
{
    SupplierPriceChange = 0,
    RegulatoryBoardOrder = 1,
    MarketAdjustment = 2,
    ManualCorrection = 3,
    Other = 4
}

public class PriceHistory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public PriceType PriceType { get; set; }
    public decimal PreviousPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal ChangeAmount => NewPrice - PreviousPrice;
    public decimal ChangePercent => PreviousPrice != 0 ? Math.Round((NewPrice - PreviousPrice) / PreviousPrice * 100, 2) : 0;
    public PriceChangeReason Reason { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
