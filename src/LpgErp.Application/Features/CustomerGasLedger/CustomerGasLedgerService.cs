using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CustomerGasLedger;

public interface ICustomerGasLedgerService
{
    Task<Result<CustomerGasLedgerDto>> GetCustomerLedgerAsync(Guid customerId, CancellationToken cancellationToken = default);
}

public class CustomerGasLedgerService : ICustomerGasLedgerService
{
    private readonly IApplicationDbContext _context;

    public CustomerGasLedgerService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CustomerGasLedgerDto>> GetCustomerLedgerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, cancellationToken);

        if (customer is null)
            return Result<CustomerGasLedgerDto>.Failure("Customer not found.");

        var salesOrders = await _context.SalesOrders
            .Where(so => so.CustomerId == customerId && !so.IsDeleted && so.Status == SalesOrderStatus.Delivered)
            .Include(so => so.Items).ThenInclude(i => i.Product)
            .OrderByDescending(so => so.OrderDate)
            .ToListAsync(cancellationToken);

        var orderIds = salesOrders.Select(so => so.Id).ToList();

        var payments = await _context.Payments
            .Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Inbound
                && p.SalesOrderId != null && orderIds.Contains(p.SalesOrderId.Value))
            .ToListAsync(cancellationToken);

        var deposits = await _context.CylinderDeposits
            .Where(d => d.CustomerId == customerId && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalGasPurchases = salesOrders
            .Where(so => so.Items.Any(i => i.Product.Type == ProductType.GasRefill))
            .Sum(so => so.Items.Where(i => i.Product.Type == ProductType.GasRefill).Sum(i => i.TotalPrice));

        var totalCylinderPurchases = salesOrders
            .Where(so => so.Items.Any(i => i.Product.Type == ProductType.EmptyCylinder || i.Product.Type == ProductType.NewPackage))
            .Sum(so => so.Items.Where(i => i.Product.Type == ProductType.EmptyCylinder || i.Product.Type == ProductType.NewPackage).Sum(i => i.TotalPrice));

        var totalPayments = payments.Sum(p => p.Amount);
        var totalDeposits = deposits.Where(d => d.Type == CylinderDepositType.Paid).Sum(d => d.Amount);
        // Charge what the customer was actually billed — the order net, after discount. Summing the line
        // totals ignored Discount and disagreed with the outstanding figure on the credit page.
        var totalPurchases = salesOrders.Sum(so => so.NetAmount);
        var outstandingBalance = totalPurchases - totalPayments;

        // Build the ledger oldest-first so each row's running balance is the balance as at that row,
        // then present newest-first. Accumulating newest-first (and applying every payment only after
        // every order) produced a running balance that matched no point in time.
        var ordered = salesOrders
            .Select(so => new LedgerEntryDto
            {
                Date = so.OrderDate,
                Description = $"Sales Order {so.OrderNumber}",
                Debit = so.NetAmount,
                Credit = 0
            })
            .Concat(payments.Select(p => new LedgerEntryDto
            {
                Date = p.PaymentDate,
                Description = $"Payment - {p.Method}",
                Debit = 0,
                Credit = p.Amount
            }))
            .OrderBy(e => e.Date)
            .ToList();

        decimal runningBalance = 0;
        foreach (var entry in ordered)
        {
            runningBalance += entry.Debit - entry.Credit;
            entry.RunningBalance = runningBalance;
        }

        // Exact inverse of the computation order, so rows sharing a timestamp still read newest-first
        // instead of being left in ascending order by a stable sort.
        var entries = Enumerable.Reverse(ordered).ToList();

        return Result<CustomerGasLedgerDto>.Success(new CustomerGasLedgerDto
        {
            CustomerId = customerId,
            CustomerName = customer.Name,
            TotalGasPurchases = totalGasPurchases,
            TotalCylinderPurchases = totalCylinderPurchases,
            TotalPayments = totalPayments,
            OutstandingBalance = outstandingBalance,
            TotalDeposits = totalDeposits,
            RecentTransactions = entries.Take(50).ToList()
        });
    }
}

public class CustomerGasLedgerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalGasPurchases { get; set; }
    public decimal TotalCylinderPurchases { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal TotalDeposits { get; set; }
    public List<LedgerEntryDto> RecentTransactions { get; set; } = [];
}

public class LedgerEntryDto
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}
