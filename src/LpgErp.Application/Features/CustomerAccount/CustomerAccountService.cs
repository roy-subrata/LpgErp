using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CustomerAccount.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CustomerAccount;

public interface ICustomerAccountService
{
    Task<Result<CustomerAccountSummaryDto>> GetSummaryAsync(Guid customerId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CustomerAccountSummaryDto>>> GetAllSummariesAsync(CancellationToken ct = default);
    Task<Result<CustomerStatementDto>> GetStatementAsync(Guid customerId, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CustomerOrderDto>>> GetOrdersAsync(Guid customerId, CancellationToken ct = default);
    Task<Result<CustomerAgingDto>> GetAgingAsync(CancellationToken ct = default);
}

/// <summary>
/// The single place that answers "what does this customer owe, hold, and have on deposit".
///
/// Credit, gas ledger and ageing each used to compute this independently, over three different
/// populations of orders, so the same customer could show three different balances.
/// </summary>
public class CustomerAccountService : ICustomerAccountService
{
    private readonly IApplicationDbContext _context;

    public CustomerAccountService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// An order is money owed once it has been committed to the customer — that is, confirmed or
    /// delivered. Drafts are not yet real and cancelled orders never were.
    /// </summary>
    /// <remarks>
    /// This deliberately includes Confirmed. Restricting receivables to Delivered meant credit sales,
    /// which sit at Confirmed until the cylinders physically go out, were counted as owing nothing —
    /// so every due in the system reported zero while real money was outstanding.
    /// </remarks>
    public static bool IsReceivable(SalesOrderStatus status) =>
        status is SalesOrderStatus.Confirmed or SalesOrderStatus.Delivered;

    private static readonly SalesOrderStatus[] ReceivableStatuses =
        [SalesOrderStatus.Confirmed, SalesOrderStatus.Delivered];

    public async Task<Result<CustomerAccountSummaryDto>> GetSummaryAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, ct);

        if (customer is null)
            return Result<CustomerAccountSummaryDto>.Failure("Customer not found.");

        return Result<CustomerAccountSummaryDto>.Success(await BuildSummaryAsync(customer, ct));
    }

    public async Task<Result<IReadOnlyList<CustomerAccountSummaryDto>>> GetAllSummariesAsync(CancellationToken ct = default)
    {
        var customers = await _context.Customers
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        var summaries = new List<CustomerAccountSummaryDto>(customers.Count);
        foreach (var customer in customers)
            summaries.Add(await BuildSummaryAsync(customer, ct));

        return Result<IReadOnlyList<CustomerAccountSummaryDto>>.Success(summaries);
    }

    public async Task<CustomerAccountSummaryDto> BuildSummaryAsync(Customer customer, CancellationToken ct = default)
    {
        // Sum the mapped columns, not the computed NetAmount — EF cannot translate an unmapped property.
        var billed = await _context.SalesOrders
            .Where(so => so.CustomerId == customer.Id && !so.IsDeleted && ReceivableStatuses.Contains(so.Status))
            .SumAsync(so => so.TotalAmount - so.Discount, ct);

        var paid = await ReceivablePaymentsQuery(customer.Id).SumAsync(p => p.Amount, ct);

        var depositsPaid = await DepositQuery(customer.Id, CylinderDepositType.Paid).SumAsync(d => d.Amount, ct);
        var depositsRefunded = await DepositQuery(customer.Id, CylinderDepositType.Refund).SumAsync(d => d.Amount, ct);

        var cylindersHeld = await _context.CustomerCylinderBalances
            .Where(b => b.CustomerId == customer.Id && !b.IsDeleted)
            .SumAsync(b => b.Received - b.Returned - b.Forfeited, ct);

        var due = billed - paid;

        return new CustomerAccountSummaryDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CreditLimit = customer.CreditLimit,
            TotalBilled = billed,
            TotalPaid = paid,
            OutstandingDue = due,
            DepositHeld = depositsPaid - depositsRefunded,
            CylindersHeld = cylindersHeld,
            CreditUtilization = customer.CreditLimit > 0
                ? Math.Round(due / customer.CreditLimit * 100, 2)
                : 0m,
            IsOverCredit = customer.CreditLimit > 0 && due > customer.CreditLimit,
        };
    }

    public async Task<Result<CustomerStatementDto>> GetStatementAsync(Guid customerId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, ct);

        if (customer is null)
            return Result<CustomerStatementDto>.Failure("Customer not found.");

        var orders = await _context.SalesOrders
            .Where(so => so.CustomerId == customerId && !so.IsDeleted && ReceivableStatuses.Contains(so.Status))
            .Select(so => new { so.Id, so.OrderNumber, so.OrderDate, so.IsCreditSale, Net = so.TotalAmount - so.Discount })
            .ToListAsync(ct);

        var payments = await CustomerPaymentsQuery(customerId)
            .Include(p => p.PaymentAccount)
            .Include(p => p.SalesOrder)
            .ToListAsync(ct);

        var lines = new List<StatementLineDto>();

        foreach (var order in orders)
        {
            lines.Add(new StatementLineDto
            {
                Date = order.OrderDate,
                Kind = StatementLineKind.Sale,
                Description = $"Sale {order.OrderNumber}{(order.IsCreditSale ? " (credit)" : "")}",
                Debit = order.Net,
            });
        }

        foreach (var payment in payments)
        {
            var channel = payment.PaymentAccount?.Name ?? payment.Method.ToString();

            // Deposits move cash but are a liability, not a settlement of goods sold, so they are
            // shown on their own line and kept out of the running balance on goods.
            var (kind, description, debit, credit) = payment.Purpose switch
            {
                PaymentPurpose.CylinderDeposit =>
                    (StatementLineKind.Deposit, $"Cylinder deposit · {channel}", 0m, 0m),
                PaymentPurpose.DepositRefund =>
                    (StatementLineKind.DepositRefund, $"Deposit refund · {channel}", 0m, 0m),
                PaymentPurpose.ExchangeCharge =>
                    (StatementLineKind.ExchangeCharge, $"Exchange charge · {channel}", payment.Amount, payment.Amount),
                _ => (StatementLineKind.Payment,
                      $"Payment · {channel}{(payment.SalesOrder != null ? $" · {payment.SalesOrder.OrderNumber}" : "")}",
                      0m, payment.Amount),
            };

            lines.Add(new StatementLineDto
            {
                Date = payment.PaymentDate,
                Kind = kind,
                Description = description,
                Debit = debit,
                Credit = credit,
                Amount = payment.Amount,
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

        var summary = await BuildSummaryAsync(customer, ct);

        var cylinders = await _context.CustomerCylinderBalances
            .Where(b => b.CustomerId == customerId && !b.IsDeleted && b.Received - b.Returned - b.Forfeited != 0)
            .Include(b => b.Brand)
            .Include(b => b.CylinderSize)
            .Select(b => new CylinderHoldingDto
            {
                BrandName = b.Brand.Name,
                CylinderSizeName = b.CylinderSize.Name,
                Held = b.Received - b.Returned - b.Forfeited,
            })
            .ToListAsync(ct);

        return Result<CustomerStatementDto>.Success(new CustomerStatementDto
        {
            Summary = summary,
            From = from,
            To = to,
            OpeningBalance = openingBalance,
            ClosingBalance = running,
            Lines = Enumerable.Reverse(ordered).ToList(),
            Cylinders = cylinders,
        });
    }

    /// <summary>
    /// The customer's orders with what is still unpaid on each — the list you work from when
    /// collecting money, rather than a total with no breakdown.
    /// </summary>
    public async Task<Result<IReadOnlyList<CustomerOrderDto>>> GetOrdersAsync(Guid customerId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var orders = await _context.SalesOrders
            .Where(so => so.CustomerId == customerId && !so.IsDeleted && so.Status != SalesOrderStatus.Cancelled)
            .OrderByDescending(so => so.OrderDate)
            .Select(so => new
            {
                so.Id,
                so.OrderNumber,
                so.OrderDate,
                so.DueDate,
                so.Status,
                so.IsCreditSale,
                Net = so.TotalAmount - so.Discount,
            })
            .ToListAsync(ct);

        var orderIds = orders.Select(o => o.Id).ToList();

        var paidByOrder = await _context.Payments
            .Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Inbound
                && p.Purpose == PaymentPurpose.OrderSettlement
                && p.SalesOrderId != null && orderIds.Contains(p.SalesOrderId.Value))
            .GroupBy(p => p.SalesOrderId!.Value)
            .Select(g => new { OrderId = g.Key, Paid = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.OrderId, x => x.Paid, ct);

        var result = orders.Select(o =>
        {
            paidByOrder.TryGetValue(o.Id, out var paid);
            var outstanding = o.Net - paid;
            return new CustomerOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                DueDate = o.DueDate,
                Status = (int)o.Status,
                IsCreditSale = o.IsCreditSale,
                NetAmount = o.Net,
                Paid = paid,
                Outstanding = outstanding,
                IsOverdue = outstanding > 0 && o.DueDate.HasValue && o.DueDate.Value < now,
            };
        }).ToList();

        return Result<IReadOnlyList<CustomerOrderDto>>.Success(result);
    }

    public async Task<Result<CustomerAgingDto>> GetAgingAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Ageing is about credit terms being exceeded, so it covers credit sales only. A cash sale
        // was never given a term to run past. (The summary still counts every unpaid order.)
        var orders = await _context.SalesOrders
            .Where(so => !so.IsDeleted && so.IsCreditSale && ReceivableStatuses.Contains(so.Status))
            .Include(so => so.Customer)
            .Select(so => new
            {
                so.Id,
                so.CustomerId,
                CustomerName = so.Customer.Name,
                so.OrderDate,
                so.DueDate,
                Net = so.TotalAmount - so.Discount,
            })
            .ToListAsync(ct);

        var settlements = await _context.Payments
            .Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Inbound
                && p.Purpose == PaymentPurpose.OrderSettlement && p.SalesOrderId != null)
            .GroupBy(p => p.SalesOrderId!.Value)
            .Select(g => new { OrderId = g.Key, Paid = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.OrderId, x => x.Paid, ct);

        var entries = new List<CustomerAgingEntryDto>();

        foreach (var group in orders.GroupBy(o => new { o.CustomerId, o.CustomerName }))
        {
            var aged = new CustomerAgingEntryDto
            {
                CustomerId = group.Key.CustomerId,
                CustomerName = group.Key.CustomerName,
            };

            foreach (var order in group)
            {
                settlements.TryGetValue(order.Id, out var paid);
                var balance = order.Net - paid;
                if (balance <= 0) continue;

                // Age from the due date when there is one — a 60-day-term invoice raised 40 days ago
                // is not overdue at all.
                var daysOverdue = (now - (order.DueDate ?? order.OrderDate)).Days;

                if (daysOverdue <= 0) aged.Current += balance;
                else if (daysOverdue <= 30) aged.Days30 += balance;
                else if (daysOverdue <= 60) aged.Days60 += balance;
                else if (daysOverdue <= 90) aged.Days90 += balance;
                else aged.DaysOver90 += balance;
            }

            if (aged.Total > 0) entries.Add(aged);
        }

        entries = entries.OrderByDescending(e => e.Total).ToList();

        return Result<CustomerAgingDto>.Success(new CustomerAgingDto
        {
            Entries = entries,
            TotalCurrent = entries.Sum(e => e.Current),
            TotalDays30 = entries.Sum(e => e.Days30),
            TotalDays60 = entries.Sum(e => e.Days60),
            TotalDays90 = entries.Sum(e => e.Days90),
            TotalDaysOver90 = entries.Sum(e => e.DaysOver90),
        });
    }

    /// <summary>
    /// Every payment belonging to a customer, however it was recorded. Matched on the customer link
    /// or through the order, because payments predating that link carry only the order reference.
    /// </summary>
    private IQueryable<Payment> CustomerPaymentsQuery(Guid customerId) =>
        _context.Payments.Where(p => !p.IsDeleted
            && (p.CustomerId == customerId
                || _context.SalesOrders.Any(so => so.Id == p.SalesOrderId
                    && so.CustomerId == customerId && !so.IsDeleted)));

    /// <summary>
    /// Payments that reduce what the customer owes on goods. Deposits are excluded — they are held
    /// against cylinders and refundable, so counting them here would understate the real debt.
    /// Scoped to receivable orders so money against a draft cannot cancel out a real debt.
    /// </summary>
    private IQueryable<Payment> ReceivablePaymentsQuery(Guid customerId) =>
        _context.Payments.Where(p => !p.IsDeleted
            && p.Direction == PaymentDirection.Inbound
            && p.Purpose == PaymentPurpose.OrderSettlement
            && (
                // Against one of the customer's receivable orders...
                _context.SalesOrders.Any(so => so.Id == p.SalesOrderId
                    && so.CustomerId == customerId && !so.IsDeleted && ReceivableStatuses.Contains(so.Status))
                // ...or paid on account, against the balance as a whole.
                || (p.SalesOrderId == null && p.CustomerId == customerId)
            ));

    private IQueryable<CylinderDeposit> DepositQuery(Guid customerId, CylinderDepositType type) =>
        _context.CylinderDeposits.Where(d => !d.IsDeleted && d.CustomerId == customerId && d.Type == type);
}
