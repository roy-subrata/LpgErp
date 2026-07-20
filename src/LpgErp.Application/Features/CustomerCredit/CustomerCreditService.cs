using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CustomerCredit;

public interface ICustomerCreditService
{
    Task<Result<CustomerCreditSummaryDto>> GetCustomerCreditSummaryAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CustomerCreditSummaryDto>>> GetAllCreditSummariesAsync(CancellationToken cancellationToken = default);
    Task<Result<CreditAgingReportDto>> GetCreditAgingAsync(CancellationToken cancellationToken = default);
}

public class CustomerCreditService : ICustomerCreditService
{
    private readonly IApplicationDbContext _context;

    public CustomerCreditService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CustomerCreditSummaryDto>> GetCustomerCreditSummaryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, cancellationToken);

        if (customer is null)
            return Result<CustomerCreditSummaryDto>.Failure("Customer not found.");

        var summary = await BuildCreditSummaryAsync(customer, cancellationToken);
        return Result<CustomerCreditSummaryDto>.Success(summary);
    }

    public async Task<Result<IReadOnlyList<CustomerCreditSummaryDto>>> GetAllCreditSummariesAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _context.Customers
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var summaries = new List<CustomerCreditSummaryDto>();
        foreach (var customer in customers)
        {
            summaries.Add(await BuildCreditSummaryAsync(customer, cancellationToken));
        }

        return Result<IReadOnlyList<CustomerCreditSummaryDto>>.Success(summaries);
    }

    public async Task<Result<CreditAgingReportDto>> GetCreditAgingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var customers = await _context.Customers
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var entries = new List<CreditAgingEntry>();

        foreach (var customer in customers)
        {
            var salesOrders = await _context.SalesOrders
                .Where(so => so.CustomerId == customer.Id && !so.IsDeleted)
                .Include(so => so.Payments.Where(p => !p.IsDeleted))
                .ToListAsync(cancellationToken);

            var orderIds = salesOrders.Select(so => so.Id).ToList();

            var payments = await _context.Payments
                .Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Inbound
                    && p.SalesOrderId != null && orderIds.Contains(p.SalesOrderId.Value))
                .ToListAsync(cancellationToken);

            var totalPurchases = salesOrders.Sum(so => so.NetAmount);
            var totalPayments = payments.Sum(p => p.Amount);
            var outstanding = totalPurchases - totalPayments;

            if (outstanding <= 0) continue;

            var aged = new CreditAgingEntry
            {
                CustomerName = customer.Name,
                Current = 0m,
                Days30 = 0m,
                Days60 = 0m,
                Days90 = 0m,
                DaysOver90 = 0m
            };

            foreach (var order in salesOrders)
            {
                var orderPayments = payments.Where(p => p.SalesOrderId == order.Id).Sum(p => p.Amount);
                var orderBalance = order.NetAmount - orderPayments;
                if (orderBalance <= 0) continue;

                var daysOld = (now - order.OrderDate).Days;

                if (daysOld <= 0)
                    aged.Current += orderBalance;
                else if (daysOld <= 30)
                    aged.Current += orderBalance;
                else if (daysOld <= 60)
                    aged.Days30 += orderBalance;
                else if (daysOld <= 90)
                    aged.Days60 += orderBalance;
                else if (daysOld <= 120)
                    aged.Days90 += orderBalance;
                else
                    aged.DaysOver90 += orderBalance;
            }

            entries.Add(aged);
        }

        return Result<CreditAgingReportDto>.Success(new CreditAgingReportDto
        {
            Entries = entries,
            TotalCurrent = entries.Sum(e => e.Current),
            TotalDays30 = entries.Sum(e => e.Days30),
            TotalDays60 = entries.Sum(e => e.Days60),
            TotalDays90 = entries.Sum(e => e.Days90),
            TotalDaysOver90 = entries.Sum(e => e.DaysOver90)
        });
    }

    private async Task<CustomerCreditSummaryDto> BuildCreditSummaryAsync(Customer customer, CancellationToken cancellationToken)
    {
        var totalPurchases = await _context.SalesOrders
            .Where(so => so.CustomerId == customer.Id && !so.IsDeleted)
            .SumAsync(so => so.NetAmount, cancellationToken);

        var totalPayments = await _context.Payments
            .Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Inbound
                && _context.SalesOrders.Any(so => so.Id == p.SalesOrderId && so.CustomerId == customer.Id && !so.IsDeleted))
            .SumAsync(p => p.Amount, cancellationToken);

        var outstandingBalance = totalPurchases - totalPayments;
        var creditUtilization = customer.CreditLimit > 0
            ? Math.Round(outstandingBalance / customer.CreditLimit * 100, 2)
            : 0m;

        return new CustomerCreditSummaryDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CreditLimit = customer.CreditLimit,
            TotalPurchases = totalPurchases,
            TotalPayments = totalPayments,
            OutstandingBalance = outstandingBalance,
            CreditUtilization = creditUtilization,
            IsOverCredit = customer.CreditLimit > 0 && outstandingBalance > customer.CreditLimit
        };
    }
}

public class CustomerCreditSummaryDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal CreditUtilization { get; set; }
    public bool IsOverCredit { get; set; }
}

public class CreditAgingReportDto
{
    public List<CreditAgingEntry> Entries { get; set; } = [];
    public decimal TotalCurrent { get; set; }
    public decimal TotalDays30 { get; set; }
    public decimal TotalDays60 { get; set; }
    public decimal TotalDays90 { get; set; }
    public decimal TotalDaysOver90 { get; set; }
}

public class CreditAgingEntry
{
    public string CustomerName { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Days30 { get; set; }
    public decimal Days60 { get; set; }
    public decimal Days90 { get; set; }
    public decimal DaysOver90 { get; set; }
}
