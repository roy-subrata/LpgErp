using LpgErp.Application.Common.Interfaces;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.Payments;

/// <summary>
/// The account a payment moved through has to agree with the method it was paid by, wherever
/// the payment is recorded — the Payments screen, a sales order, or a purchase order.
/// </summary>
public static class PaymentAccountRules
{
    /// <summary>
    /// Returns an error message, or null when the account is acceptable for this method.
    /// Cash and cheque need no account; mobile banking and bank transfers do, since without one
    /// the payment can't be reconciled against a wallet or bank statement.
    /// </summary>
    public static async Task<string?> ValidateAsync(
        IApplicationDbContext context,
        Guid? accountId,
        PaymentMethod method,
        CancellationToken cancellationToken = default)
    {
        if (accountId is not Guid id)
        {
            return method is PaymentMethod.MobileBanking or PaymentMethod.Bank
                ? "Select which account this payment went through."
                : null;
        }

        var account = await context.PaymentAccounts
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);

        if (account is null) return "Payment account not found.";
        if (!account.IsActive) return $"'{account.Name}' is inactive and cannot take new payments.";
        if (account.Method != method)
            return $"'{account.Name}' is a {account.Method} account, so it cannot record a {method} payment.";

        return null;
    }
}
