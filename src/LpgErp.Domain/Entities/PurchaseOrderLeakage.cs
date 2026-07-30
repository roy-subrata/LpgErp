namespace LpgErp.Domain.Entities;

/// <summary>How the company settled a batch of leaking cylinders sent back to them.</summary>
/// <remarks>Numbering is fixed — these values are persisted. Append new members, never renumber.</remarks>
public enum LeakageResolution
{
    /// <summary>The company refills them at no charge. Gas comes back, nothing is billed.</summary>
    FreeRefill = 0,

    /// <summary>The company takes money off the bill instead of returning anything.</summary>
    CreditAdjustment = 1,

    /// <summary>The company swaps them for good empty cylinders.</summary>
    Replacement = 2,
}

/// <summary>
/// Leaking cylinders handed back to the company with a purchase order. Kept separate from the
/// order's items: items are goods being bought, this is faulty stock going the other way.
/// </summary>
public class PurchaseOrderLeakage : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public Guid BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    public Guid CylinderSizeId { get; set; }
    public CylinderSize CylinderSize { get; set; } = null!;

    /// <summary>Leaking cylinders sent back.</summary>
    public int Quantity { get; set; }

    public LeakageResolution Resolution { get; set; }

    /// <summary>
    /// Money taken off the bill, for <see cref="LeakageResolution.CreditAdjustment"/> only.
    /// Free refills and replacements settle in goods, not cash.
    /// </summary>
    public decimal CreditAmount { get; set; }

    /// <summary>Cylinders settled so far — refilled, replaced, or credited — as the order is received.</summary>
    public int SettledQuantity { get; set; }

    public string? Notes { get; set; }
}
