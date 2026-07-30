namespace LpgErp.Domain.Entities;

/// <summary>
/// The channel a payment moved through. Which specific wallet or bank account it hit is
/// carried by <see cref="Payment.PaymentAccountId"/>.
/// </summary>
/// <remarks>
/// Numbering is fixed — these values are persisted. Append new members, never renumber.
/// </remarks>
public enum PaymentMethod
{
    Cash = 0,

    /// <summary>
    /// Legacy. Credit is a sale term (<see cref="SalesOrder.IsCreditSale"/>), not a way of paying,
    /// so it is no longer offered on new payments. Kept because existing rows store it.
    /// </summary>
    Credit = 1,

    Bank = 2,
    MobileBanking = 3,
    Cheque = 4
}

public enum PaymentDirection
{
    Inbound = 0,
    Outbound = 1
}

/// <summary>
/// Why the money moved. Previously this had to be guessed from which foreign key happened to be
/// set, which left cylinder deposits and exchange charges with nowhere to be recorded at all.
/// </summary>
/// <remarks>Numbering is fixed — these values are persisted. Append new members, never renumber.</remarks>
public enum PaymentPurpose
{
    /// <summary>Settling a sales or purchase order. The default, and what every existing row is.</summary>
    OrderSettlement = 0,

    /// <summary>Refundable security taken against cylinders. Money received, but owed back — not revenue.</summary>
    CylinderDeposit = 1,

    /// <summary>Returning a cylinder deposit to the customer.</summary>
    DepositRefund = 2,

    /// <summary>Fee charged for swapping one brand or size of cylinder for another.</summary>
    ExchangeCharge = 3,
}

/// <summary>
/// One row per movement of money, in or out, whatever caused it. Sales, purchases, cylinder
/// deposits and exchange charges all land here, so the cash and wallet balances are complete.
/// </summary>
public class Payment : BaseEntity
{
    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    /// <summary>
    /// Who the money was with. Set directly rather than reached through an order, because deposits,
    /// refunds and on-account collections belong to a customer without belonging to any one order.
    /// </summary>
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public PaymentPurpose Purpose { get; set; } = PaymentPurpose.OrderSettlement;

    /// <summary>The deposit record this cash movement settles, when the purpose is a deposit or refund.</summary>
    public Guid? CylinderDepositId { get; set; }
    public CylinderDeposit? CylinderDeposit { get; set; }

    /// <summary>The exchange this charge was collected for.</summary>
    public Guid? CylinderExchangeId { get; set; }
    public CylinderExchange? CylinderExchange { get; set; }

    public PaymentMethod Method { get; set; }

    /// <summary>Which wallet or bank account the money moved through. Null for plain cash-in-hand.</summary>
    public Guid? PaymentAccountId { get; set; }
    public PaymentAccount? PaymentAccount { get; set; }

    public PaymentDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Money the customer owes us against this payment's order. Deposits are excluded: they are a
    /// refundable liability, not sales revenue, and must not reduce what is owed on goods.
    /// </summary>
    public bool SettlesReceivable => Purpose == PaymentPurpose.OrderSettlement;
}
