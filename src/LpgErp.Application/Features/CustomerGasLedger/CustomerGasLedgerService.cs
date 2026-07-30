using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CustomerAccount;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CustomerGasLedger;

public interface ICustomerGasLedgerService
{
    Task<Result<CustomerGasLedgerDto>> GetCustomerLedgerAsync(Guid customerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Splits a customer's spend into gas versus cylinders — the one thing this ledger uniquely
/// answers. Balances and ledger lines come from <see cref="ICustomerAccountService"/> so this
/// screen can no longer disagree with the credit page about what is owed.
/// </summary>
public class CustomerGasLedgerService : ICustomerGasLedgerService
{
    private readonly IApplicationDbContext _context;
    private readonly ICustomerAccountService _accounts;

    public CustomerGasLedgerService(IApplicationDbContext context, ICustomerAccountService accounts)
    {
        _context = context;
        _accounts = accounts;
    }

    public async Task<Result<CustomerGasLedgerDto>> GetCustomerLedgerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var statement = await _accounts.GetStatementAsync(customerId, null, null, cancellationToken);
        if (!statement.IsSuccess)
            return Result<CustomerGasLedgerDto>.Failure(statement.Error!);

        var summary = statement.Data!.Summary;

        // Gas and cylinders are separate assets, so the spend on each is reported separately.
        var lines = await _context.SalesOrderItems
            .Where(i => !i.IsDeleted
                && _context.SalesOrders.Any(so => so.Id == i.SalesOrderId
                    && so.CustomerId == customerId && !so.IsDeleted
                    && (so.Status == SalesOrderStatus.Confirmed || so.Status == SalesOrderStatus.Delivered)))
            .Select(i => new { i.Product.Type, i.TotalPrice })
            .ToListAsync(cancellationToken);

        return Result<CustomerGasLedgerDto>.Success(new CustomerGasLedgerDto
        {
            CustomerId = summary.CustomerId,
            CustomerName = summary.CustomerName,
            TotalGasPurchases = lines.Where(l => l.Type == ProductType.GasRefill).Sum(l => l.TotalPrice),
            TotalCylinderPurchases = lines
                .Where(l => l.Type is ProductType.EmptyCylinder or ProductType.NewPackage)
                .Sum(l => l.TotalPrice),
            TotalPayments = summary.TotalPaid,
            OutstandingBalance = summary.OutstandingDue,
            TotalDeposits = summary.DepositHeld,
            RecentTransactions = statement.Data!.Lines
                .Take(50)
                .Select(l => new LedgerEntryDto
                {
                    Date = l.Date,
                    Description = l.Description,
                    Debit = l.Debit,
                    Credit = l.Credit,
                    RunningBalance = l.RunningBalance,
                })
                .ToList(),
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
