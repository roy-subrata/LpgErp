using AutoMapper;
using FluentAssertions;
using LpgErp.Application.Common.Mappings;
using LpgErp.Application.Features.CustomerCredit;
using LpgErp.Application.Features.SalesOrders;
using LpgErp.Application.Features.SalesOrders.DTOs;
using LpgErp.Application.Features.StockTransfer;
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
/// Guards on values that reach the write paths from outside. Each of these was accepted at one point
/// and wrote figures into the books that could not be produced by any real transaction.
/// </summary>
public class InputGuardTests
{
    private readonly InMemoryDatabaseRoot _root = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()), NullLoggerFactory.Instance).CreateMapper();

    private LpgErpDbContext NewContext() =>
        new(new DbContextOptionsBuilder<LpgErpDbContext>()
            .UseInMemoryDatabase("input-guards", _root)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private SalesOrderService SalesSvc(LpgErpDbContext c) => new(c, new UnitOfWork(c), _mapper);
    private StockTransferService TransferSvc(LpgErpDbContext c) => new(c, new UnitOfWork(c));

    private Guid _customerId, _warehouseAId, _warehouseBId, _productId;

    private async Task SeedAsync()
    {
        using var context = NewContext();
        var customer = new Customer { Name = "Hotel Star", CreditLimit = 100_000m };
        var warehouseA = new Warehouse { Name = "Main" };
        var warehouseB = new Warehouse { Name = "City" };
        var product = new Product { Name = "12KG Package", Type = ProductType.NewPackage, CurrentStock = 100, SalePrice = 1200m };

        context.Customers.Add(customer);
        context.Warehouses.AddRange(warehouseA, warehouseB);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        context.StockLevels.Add(new StockLevel { WarehouseId = warehouseA.Id, ProductId = product.Id, Quantity = 100 });
        await context.SaveChangesAsync();

        _customerId = customer.Id;
        _warehouseAId = warehouseA.Id;
        _warehouseBId = warehouseB.Id;
        _productId = product.Id;
    }

    private CreateSalesOrderRequest OrderRequest(decimal discount) => new()
    {
        CustomerId = _customerId,
        WarehouseId = _warehouseAId,
        Discount = discount,
        Items = [new CreateSalesOrderItemRequest { ProductId = _productId, Quantity = 1, UnitPrice = 100m }]
    };

    [Fact]
    public async Task Discount_exceeding_the_order_total_is_rejected()
    {
        await SeedAsync();
        using var context = NewContext();

        var result = await SalesSvc(context).CreateAsync(OrderRequest(999_999m));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cannot exceed the order total");
        (await context.SalesOrders.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Negative_discount_is_rejected()
    {
        await SeedAsync();
        using var context = NewContext();

        var result = await SalesSvc(context).CreateAsync(OrderRequest(-50m));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("negative");
    }

    [Fact]
    public async Task Discount_equal_to_the_order_total_is_allowed()
    {
        await SeedAsync();
        using var context = NewContext();

        var result = await SalesSvc(context).CreateAsync(OrderRequest(100m));

        result.IsSuccess.Should().BeTrue();
        result.Data!.NetAmount.Should().Be(0m);
    }

    [Fact]
    public async Task Negative_transfer_quantity_is_rejected_and_leaves_stock_untouched()
    {
        await SeedAsync();
        using var context = NewContext();

        var result = await TransferSvc(context).TransferAsync(
            new StockTransferRequest(_productId, _warehouseAId, _warehouseBId, -50, "test"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("greater than zero");

        // The source must not have been credited, nor the destination debited.
        var source = await context.StockLevels.FirstAsync(s => s.WarehouseId == _warehouseAId);
        source.Quantity.Should().Be(100);
        (await context.StockLevels.AnyAsync(s => s.WarehouseId == _warehouseBId)).Should().BeFalse();
        (await context.StockMovements.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Transfer_to_the_same_warehouse_is_rejected()
    {
        await SeedAsync();
        using var context = NewContext();

        var result = await TransferSvc(context).TransferAsync(
            new StockTransferRequest(_productId, _warehouseAId, _warehouseAId, 5, "test"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("must be different");
        (await context.StockMovements.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Valid_transfer_still_moves_stock()
    {
        await SeedAsync();
        using var context = NewContext();

        var result = await TransferSvc(context).TransferAsync(
            new StockTransferRequest(_productId, _warehouseAId, _warehouseBId, 40, "test"));

        result.IsSuccess.Should().BeTrue();
        (await context.StockLevels.FirstAsync(s => s.WarehouseId == _warehouseAId)).Quantity.Should().Be(60);
        (await context.StockLevels.FirstAsync(s => s.WarehouseId == _warehouseBId)).Quantity.Should().Be(40);
    }

    [Fact]
    public async Task Credit_summary_sums_net_amount_without_a_client_side_evaluation()
    {
        await SeedAsync();
        using var context = NewContext();

        var created = await SalesSvc(context).CreateAsync(OrderRequest(25m));
        created.IsSuccess.Should().BeTrue();

        // Only delivered orders are a debt, so the order has to reach delivery to count.
        var order = await context.SalesOrders.FirstAsync(o => o.Id == created.Data!.Id);
        order.Status = SalesOrderStatus.Delivered;
        await context.SaveChangesAsync();

        // Regression: this used to Sum the unmapped SalesOrder.NetAmount and throw on translation.
        var summary = await new CustomerCreditService(context).GetCustomerCreditSummaryAsync(_customerId);

        summary.IsSuccess.Should().BeTrue();
        summary.Data!.TotalPurchases.Should().Be(75m);
        summary.Data.OutstandingBalance.Should().Be(75m);
    }
}
