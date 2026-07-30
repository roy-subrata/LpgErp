namespace LpgErp.Domain.Entities;

/// <summary>
/// A concrete place money moves through — a specific bKash wallet, Nagad wallet, or bank account.
/// <see cref="PaymentMethod"/> says what kind of channel it is; this says which one, so a day's
/// takings can be reconciled against the matching wallet or bank statement.
/// </summary>
public class PaymentAccount : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>The channel category this account belongs to. A payment's method must match its account's.</summary>
    public PaymentMethod Method { get; set; }

    /// <summary>Wallet number for mobile banking, account number for a bank. Null for plain cash.</summary>
    public string? AccountNumber { get; set; }

    /// <summary>Bank or MFS provider name — "bKash", "Nagad", "City Bank".</summary>
    public string? Provider { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public ICollection<Payment> Payments { get; set; } = [];
}
