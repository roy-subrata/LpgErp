using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CustomerAccount;
using LpgErp.Application.Features.CustomerAccount.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CustomerCredit;

public interface ICustomerCreditService
{
    Task<Result<CustomerCreditSummaryDto>> GetCustomerCreditSummaryAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CustomerCreditSummaryDto>>> GetAllCreditSummariesAsync(CancellationToken cancellationToken = default);
    Task<Result<CreditAgingReportDto>> GetCreditAgingAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Credit limits and ageing, presented in the shape the existing screens expect. The balances
/// themselves come from <see cref="ICustomerAccountService"/> so this cannot drift from the
/// statement or the gas ledger the way three separate calculations did.
/// </summary>
public class CustomerCreditService : ICustomerCreditService
{
    private readonly IApplicationDbContext _context;
    private readonly ICustomerAccountService _accounts;

    public CustomerCreditService(IApplicationDbContext context, ICustomerAccountService accounts)
    {
        _context = context;
        _accounts = accounts;
    }

    public async Task<Result<CustomerCreditSummaryDto>> GetCustomerCreditSummaryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var summary = await _accounts.GetSummaryAsync(customerId, cancellationToken);
        if (!summary.IsSuccess)
            return Result<CustomerCreditSummaryDto>.Failure(summary.Error!);

        return Result<CustomerCreditSummaryDto>.Success(ToCreditSummary(summary.Data!));
    }

    public async Task<Result<IReadOnlyList<CustomerCreditSummaryDto>>> GetAllCreditSummariesAsync(CancellationToken cancellationToken = default)
    {
        var summaries = await _accounts.GetAllSummariesAsync(cancellationToken);
        if (!summaries.IsSuccess)
            return Result<IReadOnlyList<CustomerCreditSummaryDto>>.Failure(summaries.Error!);

        return Result<IReadOnlyList<CustomerCreditSummaryDto>>.Success(
            summaries.Data!.Select(ToCreditSummary).ToList());
    }

    private static CustomerCreditSummaryDto ToCreditSummary(CustomerAccountSummaryDto s) => new()
    {
        CustomerId = s.CustomerId,
        CustomerName = s.CustomerName,
        CreditLimit = s.CreditLimit,
        TotalPurchases = s.TotalBilled,
        TotalPayments = s.TotalPaid,
        OutstandingBalance = s.OutstandingDue,
        CreditUtilization = s.CreditUtilization,
        IsOverCredit = s.IsOverCredit,
    };

    public async Task<Result<CreditAgingReportDto>> GetCreditAgingAsync(CancellationToken cancellationToken = default)
    {
        var aging = await _accounts.GetAgingAsync(cancellationToken);
        if (!aging.IsSuccess)
            return Result<CreditAgingReportDto>.Failure(aging.Error!);

        var data = aging.Data!;
        return Result<CreditAgingReportDto>.Success(new CreditAgingReportDto
        {
            Entries = data.Entries.Select(e => new CreditAgingEntry
            {
                CustomerName = e.CustomerName,
                Current = e.Current,
                Days30 = e.Days30,
                Days60 = e.Days60,
                Days90 = e.Days90,
                DaysOver90 = e.DaysOver90,
            }).ToList(),
            TotalCurrent = data.TotalCurrent,
            TotalDays30 = data.TotalDays30,
            TotalDays60 = data.TotalDays60,
            TotalDays90 = data.TotalDays90,
            TotalDaysOver90 = data.TotalDaysOver90,
        });
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
