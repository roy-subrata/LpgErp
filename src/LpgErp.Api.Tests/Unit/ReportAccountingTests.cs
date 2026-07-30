using AutoMapper;
using FluentAssertions;
using LpgErp.Application.Common.Mappings;
using LpgErp.Application.Features.CustomerAccount;
using LpgErp.Application.Features.CustomerCredit;
using LpgErp.Application.Features.CustomerGasLedger;
using LpgErp.Application.Features.Reports;
using LpgErp.Domain.Entities;
using LpgErp.Infrastructure.Persistence;
using LpgErp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LpgErp.Api.Tests.Unit;

/// <summary>
/// The accounting rules the money reports rest on: revenue is recognised on delivery, a returned
/// deposit is no longer a liability, and a ledger balance means the balance as at that row.
/// </summary>
public class ReportAccountingTests
{
    private readonly InMemoryDatabaseRoot _root = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()), NullLoggerFactory.Instance).CreateMapper();

    private LpgErpDbContext NewContext() =>
        new(new DbContextOptionsBuilder<LpgErpDbContext>()
            .UseInMemoryDatabase($"report-accounting-{_dbName}", _root)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly DateTime _from = new(2026, 1, 1);
    private readonly DateTime _to = new(2026, 12, 31);

    private Guid _customerId, _warehouseId, _productId, _sizeId;

    private async Task<LpgErpDbContext> SeedAsync()
    {
        var context = NewContext();
        var customer = new Customer { Name = "Karim Traders", CreditLimit = 50_000m };
        var warehouse = new Warehouse { Name = "Main" };
        var size = new CylinderSize { Name = "12 KG" };
        var product = new Product { Name = "12KG Refill", Type = ProductType.GasRefill, SalePrice = 1000m };
        context.AddRange(customer, warehouse, size, product);
        await context.SaveChangesAsync();

        _customerId = customer.Id;
        _warehouseId = warehouse.Id;
        _productId = product.Id;
        _sizeId = size.Id;
        return context;
    }

    private SalesOrder Order(SalesOrderStatus status, decimal total, decimal discount = 0, bool credit = false, DateTime? date = null) => new()
    {
        OrderNumber = $"SO-{Guid.NewGuid().ToString()[..6]}",
        CustomerId = _customerId,
        WarehouseId = _warehouseId,
        Status = status,
        TotalAmount = total,
        Discount = discount,
        IsCreditSale = credit,
        OrderDate = date ?? new DateTime(2026, 6, 1)
    };

    [Fact]
    public async Task Financial_report_recognises_revenue_on_delivery_but_receivable_includes_confirmed()
    {
        using var context = await SeedAsync();
        context.SalesOrders.AddRange(
            Order(SalesOrderStatus.Delivered, 1000m),
            Order(SalesOrderStatus.Confirmed, 500m),
            Order(SalesOrderStatus.Draft, 300m),
            Order(SalesOrderStatus.Cancelled, 200m));
        await context.SaveChangesAsync();

        var result = await new ReportService(context, new CustomerAccountService(context)).GetFinancialReportAsync(_from, _to);

        // Revenue is recognised on delivery — the draft and cancelled orders never count, and the
        // confirmed one hasn't shipped yet.
        result.Data!.TotalSales.Should().Be(1000m);

        // Receivable is a balance-sheet snapshot of what's owed right now, so it also includes the
        // confirmed credit sale sitting un-delivered — money owed doesn't wait for the truck to leave.
        result.Data.AccountsReceivable.Should().Be(1500m);
    }

    [Fact]
    public async Task Receivable_includes_confirmed_credit_sales_not_yet_delivered()
    {
        using var context = await SeedAsync();
        var delivered = Order(SalesOrderStatus.Delivered, 1000m);
        var confirmed = Order(SalesOrderStatus.Confirmed, 5000m);
        context.SalesOrders.AddRange(delivered, confirmed);
        await context.SaveChangesAsync();

        context.Payments.AddRange(
            new Payment { SalesOrderId = delivered.Id, Direction = PaymentDirection.Inbound, Amount = 400m, PaymentDate = new DateTime(2026, 6, 2) },
            // Partial payment up front on an order that hasn't shipped — still owed, so still a receivable.
            new Payment { SalesOrderId = confirmed.Id, Direction = PaymentDirection.Inbound, Amount = 3000m, PaymentDate = new DateTime(2026, 6, 2) });
        await context.SaveChangesAsync();

        var report = (await new ReportService(context, new CustomerAccountService(context)).GetFinancialReportAsync(_from, _to)).Data!;

        report.TotalSales.Should().Be(1000m);          // revenue recognised on delivery only
        report.TotalPayments.Should().Be(3400m);        // all cash collected
        report.AccountsReceivable.Should().Be(2600m);   // (1000 + 5000) - (400 + 3000)
    }

    [Fact]
    public async Task Deposit_liability_nets_off_returned_and_refunded_deposits()
    {
        using var context = await SeedAsync();
        context.CylinderDeposits.AddRange(
            new CylinderDeposit { CustomerId = _customerId, CylinderSizeId = _sizeId, Type = CylinderDepositType.Paid, Amount = 5000m },
            new CylinderDeposit { CustomerId = _customerId, CylinderSizeId = _sizeId, Type = CylinderDepositType.Returned, Amount = 1500m },
            new CylinderDeposit { CustomerId = _customerId, CylinderSizeId = _sizeId, Type = CylinderDepositType.Refund, Amount = 500m });
        await context.SaveChangesAsync();

        var result = await new ReportService(context, new CustomerAccountService(context)).GetFinancialReportAsync(_from, _to);

        // 5000 held, 2000 given back.
        result.Data!.DepositLiability.Should().Be(3000m);
    }

    [Fact]
    public async Task Net_profit_includes_the_settlement_costs_the_pnl_breakdown_reports()
    {
        using var context = await SeedAsync();
        var driver = new Driver { Name = "Karim" };
        var salesman = new Salesman { Name = "Arif" };
        context.AddRange(driver, salesman);
        context.SalesOrders.Add(Order(SalesOrderStatus.Delivered, 10_000m));
        await context.SaveChangesAsync();

        var date = new DateTime(2026, 6, 2);
        context.DriverSettlements.Add(new DriverSettlement { DriverId = driver.Id, SettlementDate = date, FuelCost = 400m, Allowance = 300m });
        context.SalesmanSettlements.Add(new SalesmanSettlement { SalesmanId = salesman.Id, SettlementDate = date, Commission = 250m, Bonus = 100m, DailyAllowance = 200m });
        await context.SaveChangesAsync();

        var svc = new ReportService(context, new CustomerAccountService(context));
        var financial = (await svc.GetFinancialReportAsync(_from, _to)).Data!;
        var pnl = (await svc.GetPnLBreakdownAsync(_from, _to)).Data!;

        financial.OperatingExpenses.Should().Be(850m); // 300 driver allowance + 250 + 100 + 200
        financial.NetProfit.Should().Be(10_000m - 400m - 850m);

        // The two screens must agree.
        var pnlNet = pnl.Where(c => c.IsIncome).Sum(c => c.Amount) - pnl.Where(c => !c.IsIncome).Sum(c => c.Amount);
        pnlNet.Should().Be(financial.NetProfit);
    }

    [Fact]
    public async Task Ageing_buckets_hold_the_days_overdue_range_they_are_named_for()
    {
        using var context = await SeedAsync();
        var now = DateTime.UtcNow;
        // Each order is delivered, on credit, and unpaid; DueDate drives the bucket.
        context.SalesOrders.AddRange(
            Order(SalesOrderStatus.Delivered, 100m, credit: true).DueOn(now.AddDays(10)),   // not due yet
            Order(SalesOrderStatus.Delivered, 200m, credit: true).DueOn(now.AddDays(-15)),  // 15 days over
            Order(SalesOrderStatus.Delivered, 400m, credit: true).DueOn(now.AddDays(-45)),  // 45 days over
            Order(SalesOrderStatus.Delivered, 800m, credit: true).DueOn(now.AddDays(-75)),  // 75 days over
            Order(SalesOrderStatus.Delivered, 1600m, credit: true).DueOn(now.AddDays(-100)) // 100 days over
        );
        await context.SaveChangesAsync();

        var aging = (await new CustomerCreditService(context, new CustomerAccountService(context)).GetCreditAgingAsync()).Data!;

        aging.TotalCurrent.Should().Be(100m);
        aging.TotalDays30.Should().Be(200m);
        aging.TotalDays60.Should().Be(400m);
        aging.TotalDays90.Should().Be(800m);
        aging.TotalDaysOver90.Should().Be(1600m); // used to land in the "90 days" bucket
    }

    [Fact]
    public async Task Ageing_ignores_cash_sales()
    {
        using var context = await SeedAsync();
        context.SalesOrders.Add(Order(SalesOrderStatus.Delivered, 5000m, credit: false).DueOn(DateTime.UtcNow.AddDays(-40)));
        await context.SaveChangesAsync();

        var aging = (await new CustomerCreditService(context, new CustomerAccountService(context)).GetCreditAgingAsync()).Data!;

        aging.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task Gas_ledger_running_balance_is_the_balance_as_at_each_row()
    {
        using var context = await SeedAsync();
        var order1 = Order(SalesOrderStatus.Delivered, 1000m, date: new DateTime(2026, 6, 1));
        var order2 = Order(SalesOrderStatus.Delivered, 500m, date: new DateTime(2026, 6, 10));
        context.SalesOrders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        context.Payments.Add(new Payment
        {
            SalesOrderId = order1.Id,
            Direction = PaymentDirection.Inbound,
            Method = PaymentMethod.Cash,
            Amount = 400m,
            PaymentDate = new DateTime(2026, 6, 5)
        });
        await context.SaveChangesAsync();

        var ledger = (await new CustomerGasLedgerService(context, new CustomerAccountService(context)).GetCustomerLedgerAsync(_customerId)).Data!;

        // Newest first for display; balances read chronologically as 1000 -> 600 -> 1100.
        var rows = ledger.RecentTransactions;
        rows.Should().HaveCount(3);
        rows[0].RunningBalance.Should().Be(1100m); // 10 Jun, after the second order
        rows[1].RunningBalance.Should().Be(600m);  //  5 Jun, after the payment
        rows[2].RunningBalance.Should().Be(1000m); //  1 Jun, first order
        ledger.OutstandingBalance.Should().Be(1100m);
    }

    [Fact]
    public async Task Gas_ledger_charges_the_discounted_amount()
    {
        using var context = await SeedAsync();
        context.SalesOrders.Add(Order(SalesOrderStatus.Delivered, 1000m, discount: 250m));
        await context.SaveChangesAsync();

        var ledger = (await new CustomerGasLedgerService(context, new CustomerAccountService(context)).GetCustomerLedgerAsync(_customerId)).Data!;

        ledger.OutstandingBalance.Should().Be(750m);
    }
}

internal static class OrderDueDateExtensions
{
    /// <summary>Sets the due date fluently so the ageing cases stay readable.</summary>
    public static SalesOrder DueOn(this SalesOrder order, DateTime dueDate)
    {
        order.DueDate = dueDate;
        return order;
    }
}
