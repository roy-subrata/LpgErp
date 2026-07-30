using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.SupplierAccount.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.SupplierAccount;

public interface ISupplierAccountService
{
    Task<Result<SupplierAccountSummaryDto>> GetSummaryAsync(Guid supplierId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<SupplierAccountSummaryDto>>> GetAllSummariesAsync(CancellationToken ct = default);
    Task<Result<SupplierStatementDto>> GetStatementAsync(Guid supplierId, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<SupplierOrderDto>>> GetOrdersAsync(Guid supplierId, CancellationToken ct = default);
}

/// <summary>The single place that answers "what do we owe this supplier, and what have we paid".</summary>
public class SupplierAccountService : ISupplierAccountService
{
    private readonly IApplicationDbContext _context;

    public SupplierAccountService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// An order is money owed once it has been committed to the supplier — confirmed or beyond.
    /// Drafts are not yet real and cancelled orders never were.
    /// </summary>
    private static readonly PurchaseOrderStatus[] PayableStatuses =
        [PurchaseOrderStatus.Confirmed, PurchaseOrderStatus.InTransit, PurchaseOrderStatus.PartiallyReceived, PurchaseOrderStatus.Received];

    public async Task<Result<SupplierAccountSummaryDto>> GetSummaryAsync(Guid supplierId, CancellationToken ct = default)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId && !s.IsDeleted, ct);
        if (supplier is null) return Result<SupplierAccountSummaryDto>.Failure("Supplier not found.");

        return Result<SupplierAccountSummaryDto>.Success(await BuildSummaryAsync(supplier, ct));
    }

    public async Task<Result<IReadOnlyList<SupplierAccountSummaryDto>>> GetAllSummariesAsync(CancellationToken ct = default)
    {
        var suppliers = await _context.Suppliers
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        var summaries = new List<SupplierAccountSummaryDto>(suppliers.Count);
        foreach (var supplier in suppliers)
            summaries.Add(await BuildSummaryAsync(supplier, ct));

        return Result<IReadOnlyList<SupplierAccountSummaryDto>>.Success(summaries);
    }

    public async Task<SupplierAccountSummaryDto> BuildSummaryAsync(Supplier supplier, CancellationToken ct = default)
    {
        // Sum the mapped columns, not the computed NetPayable — EF cannot translate an unmapped property.
        var purchased = await _context.PurchaseOrders
            .Where(po => po.SupplierId == supplier.Id && !po.IsDeleted && PayableStatuses.Contains(po.Status))
            .SumAsync(po => po.TotalAmount + po.TransportationCost - po.CommissionApplied - po.LeakageCredit, ct);

        var paid = await PayablePaymentsQuery(supplier.Id).SumAsync(p => p.Amount, ct);

        var commissionEarnedLifetime = await _context.PurchaseOrders
            .Where(po => po.SupplierId == supplier.Id && !po.IsDeleted)
            .SumAsync(po => po.CommissionEarned, ct);

        return new SupplierAccountSummaryDto
        {
            SupplierId = supplier.Id,
            SupplierName = supplier.Name,
            TotalPurchased = purchased,
            TotalPaid = paid,
            OutstandingDue = purchased - paid,
            CommissionBalance = supplier.CommissionBalance,
            CommissionEarnedLifetime = commissionEarnedLifetime,
        };
    }

    public async Task<Result<SupplierStatementDto>> GetStatementAsync(Guid supplierId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId && !s.IsDeleted, ct);
        if (supplier is null) return Result<SupplierStatementDto>.Failure("Supplier not found.");

        var orders = await _context.PurchaseOrders
            .Where(po => po.SupplierId == supplierId && !po.IsDeleted && PayableStatuses.Contains(po.Status))
            .Select(po => new
            {
                po.Id,
                po.OrderNumber,
                po.OrderDate,
                Net = po.TotalAmount + po.TransportationCost - po.CommissionApplied - po.LeakageCredit,
            })
            .ToListAsync(ct);

        var payments = await PayablePaymentsQuery(supplierId)
            .Include(p => p.PaymentAccount)
            .Include(p => p.PurchaseOrder)
            .ToListAsync(ct);

        var lines = new List<SupplierStatementLineDto>();

        foreach (var order in orders)
        {
            lines.Add(new SupplierStatementLineDto
            {
                Date = order.OrderDate ?? DateTime.UtcNow,
                Kind = SupplierStatementLineKind.Purchase,
                Description = $"Purchase {order.OrderNumber}",
                Debit = order.Net,
            });
        }

        foreach (var payment in payments)
        {
            var channel = payment.PaymentAccount?.Name ?? payment.Method.ToString();
            lines.Add(new SupplierStatementLineDto
            {
                Date = payment.PaymentDate,
                Kind = SupplierStatementLineKind.Payment,
                Description = $"Payment · {channel}{(payment.PurchaseOrder != null ? $" · {payment.PurchaseOrder.OrderNumber}" : "")}",
                Credit = payment.Amount,
                Reference = payment.Reference,
            });
        }

        // Build oldest-first so each running balance is the balance as at that line.
        var ordered = lines.OrderBy(l => l.Date).ThenBy(l => l.Kind).ToList();

        var openingBalance = 0m;
        if (from is DateTime start)
        {
            openingBalance = ordered.Where(l => l.Date < start).Sum(l => l.Debit - l.Credit);
            ordered = ordered.Where(l => l.Date >= start).ToList();
        }
        if (to is DateTime end)
            ordered = ordered.Where(l => l.Date <= end).ToList();

        var running = openingBalance;
        foreach (var line in ordered)
        {
            running += line.Debit - line.Credit;
            line.RunningBalance = running;
        }

        var summary = await BuildSummaryAsync(supplier, ct);

        return Result<SupplierStatementDto>.Success(new SupplierStatementDto
        {
            Summary = summary,
            From = from,
            To = to,
            OpeningBalance = openingBalance,
            ClosingBalance = running,
            Lines = Enumerable.Reverse(ordered).ToList(),
        });
    }

    /// <summary>
    /// The supplier's purchase orders with what is still unpaid on each — the list you work from
    /// when settling money, rather than a total with no breakdown.
    /// </summary>
    public async Task<Result<IReadOnlyList<SupplierOrderDto>>> GetOrdersAsync(Guid supplierId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var orders = await _context.PurchaseOrders
            .Where(po => po.SupplierId == supplierId && !po.IsDeleted && po.Status != PurchaseOrderStatus.Cancelled)
            .OrderByDescending(po => po.OrderDate)
            .Select(po => new
            {
                po.Id,
                po.OrderNumber,
                po.OrderDate,
                po.DueDate,
                po.Status,
                po.TotalAmount,
                po.CommissionApplied,
                Net = po.TotalAmount + po.TransportationCost - po.CommissionApplied - po.LeakageCredit,
            })
            .ToListAsync(ct);

        var orderIds = orders.Select(o => o.Id).ToList();

        var paidByOrder = await _context.Payments
            .Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Outbound
                && p.PurchaseOrderId != null && orderIds.Contains(p.PurchaseOrderId.Value))
            .GroupBy(p => p.PurchaseOrderId!.Value)
            .Select(g => new { OrderId = g.Key, Paid = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.OrderId, x => x.Paid, ct);

        var result = orders.Select(o =>
        {
            paidByOrder.TryGetValue(o.Id, out var paid);
            var outstanding = o.Net - paid;
            return new SupplierOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                DueDate = o.DueDate,
                Status = (int)o.Status,
                TotalAmount = o.TotalAmount,
                CommissionApplied = o.CommissionApplied,
                NetPayable = o.Net,
                Paid = paid,
                Outstanding = outstanding,
                IsOverdue = outstanding > 0 && o.DueDate.HasValue && o.DueDate.Value < now,
            };
        }).ToList();

        return Result<IReadOnlyList<SupplierOrderDto>>.Success(result);
    }

    /// <summary>
    /// Payments that reduce what is owed to the supplier — outbound, against one of their purchase
    /// orders. Purchase-order payments have no "on account" form (unlike customer collections),
    /// so every one is tied to an order.
    /// </summary>
    private IQueryable<Payment> PayablePaymentsQuery(Guid supplierId) =>
        _context.Payments.Where(p => !p.IsDeleted
            && p.Direction == PaymentDirection.Outbound
            && p.PurchaseOrderId != null
            && _context.PurchaseOrders.Any(po => po.Id == p.PurchaseOrderId
                && po.SupplierId == supplierId && !po.IsDeleted && PayableStatuses.Contains(po.Status)));
}
