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
            .Where(so => so.CustomerId == customerId && !so.IsDeleted)
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
        var totalPurchases = totalGasPurchases + totalCylinderPurchases;
        var outstandingBalance = totalPurchases - totalPayments;

        var entries = new List<LedgerEntryDto>();
        decimal runningBalance = 0;

        foreach (var order in salesOrders.OrderByDescending(so => so.OrderDate))
        {
            runningBalance += order.NetAmount;
            entries.Add(new LedgerEntryDto
            {
                Date = order.OrderDate,
                Description = $"Sales Order {order.OrderNumber}",
                Debit = order.NetAmount,
                Credit = 0,
                RunningBalance = runningBalance
            });
        }

        foreach (var payment in payments.OrderByDescending(p => p.PaymentDate))
        {
            runningBalance -= payment.Amount;
            entries.Add(new LedgerEntryDto
            {
                Date = payment.PaymentDate,
                Description = $"Payment - {payment.Method}",
                Debit = 0,
                Credit = payment.Amount,
                RunningBalance = runningBalance
            });
        }

        entries = entries.OrderByDescending(e => e.Date).ToList();

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
