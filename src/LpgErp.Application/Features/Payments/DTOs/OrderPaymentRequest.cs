using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.Payments.DTOs;

/// <summary>
/// How an order was settled, captured at the moment the order is created. Optional — leave it off
/// for an unpaid or credit order and record the payment later from the Payments screen.
/// </summary>
public class OrderPaymentRequest
{
    /// <summary>Amount handed over now. Zero or less means nothing was paid, and no payment is recorded.</summary>
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;

    /// <summary>Which wallet or bank account took the money. Required for mobile banking and bank transfers.</summary>
    public Guid? PaymentAccountId { get; set; }

    /// <summary>Transaction id, cheque number, or receipt number.</summary>
    public string? Reference { get; set; }
}
