using AutoMapper;
using FluentAssertions;
using LpgErp.Application.Common.Mappings;
using LpgErp.Application.Features.AdvanceRefills;
using LpgErp.Application.Features.AdvanceRefills.DTOs;
using LpgErp.Application.Features.CustomerCylinderLedger;
using LpgErp.Application.Features.SalesOrders;
using LpgErp.Application.Features.SalesOrders.DTOs;
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
/// The "customer takes gas without giving an empty" scenario: one sales flow records money due
/// (credit) AND cylinder due (ledger), and later empty returns can settle the credit.
/// </summary>
public class CylinderLedgerFlowTests
{
    private readonly InMemoryDatabaseRoot _root = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()), NullLoggerFactory.Instance).CreateMapper();

    private LpgErpDbContext NewContext() =>
        new(new DbContextOptionsBuilder<LpgErpDbContext>()
            .UseInMemoryDatabase("cylinder-ledger", _root)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private SalesOrderService SalesSvc(LpgErpDbContext c) => new(c, new UnitOfWork(c), _mapper);
    private AdvanceRefillService AdvanceSvc(LpgErpDbContext c) => new(c, SalesSvc(c), _mapper);
    private CustomerCylinderLedgerService LedgerSvc(LpgErpDbContext c) => new(c, new UnitOfWork(c), _mapper);

    private Guid _customerId, _warehouseId, _brandId, _sizeId, _refillId, _packageId;

    private async Task SeedAsync(int stock = 100)
    {
        using var context = NewContext();
        var brand = new Brand { Name = "Bashundhara", Code = "BSH" };
        var size = new CylinderSize { Name = "12 KG" };
        context.Brands.Add(brand);
        context.CylinderSizes.Add(size);
        await context.SaveChangesAsync();

        var customer = new Customer { Name = "Hotel Star", PaymentDueDays = 30 };
        var warehouse = new Warehouse { Name = "Main" };
        var refill = new Product { Name = "12KG Refill", Type = ProductType.GasRefill, BrandId = brand.Id, CylinderSizeId = size.Id, SalePrice = 1500m, CurrentStock = stock };
        var package = new Product { Name = "12KG Package", Type = ProductType.NewPackage, BrandId = brand.Id, CylinderSizeId = size.Id, SalePrice = 4500m, CurrentStock = stock };
        context.Customers.Add(customer);
        context.Warehouses.Add(warehouse);
        context.Products.AddRange(refill, package);
        await context.SaveChangesAsync();

        context.StockLevels.AddRange(
            new StockLevel { WarehouseId = warehouse.Id, ProductId = refill.Id, Quantity = stock },
            new StockLevel { WarehouseId = warehouse.Id, ProductId = package.Id, Quantity = stock });
        await context.SaveChangesAsync();

        _customerId = customer.Id;
        _warehouseId = warehouse.Id;
        _brandId = brand.Id;
        _sizeId = size.Id;
        _refillId = refill.Id;
        _packageId = package.Id;
    }

    private async Task<Guid> SellAsync(Guid productId, int qty, int? emptiesReturned, bool credit = false)
    {
        using var c = NewContext();
        var svc = SalesSvc(c);
        var created = await svc.CreateAsync(new CreateSalesOrderRequest
        {
            CustomerId = _customerId,
            WarehouseId = _warehouseId,
            IsCreditSale = credit,
            Items = [new CreateSalesOrderItemRequest { ProductId = productId, Quantity = qty, UnitPrice = 1500m, EmptyReturnedQuantity = emptiesReturned }]
        });
        await svc.ConfirmAsync(created.Data!.Id);
        (await svc.DeliverAsync(created.Data.Id)).IsSuccess.Should().BeTrue();
        return created.Data.Id;
    }

    private async Task<CustomerCylinderBalance?> BalanceAsync()
    {
        using var c = NewContext();
        return await c.CustomerCylinderBalances
            .FirstOrDefaultAsync(b => b.CustomerId == _customerId && b.BrandId == _brandId && b.CylinderSizeId == _sizeId);
    }

    [Fact]
    public async Task Refill_DefaultFullSwap_NoOutstanding()
    {
        await SeedAsync();
        await SellAsync(_refillId, 5, emptiesReturned: null); // blank = swap

        var balance = await BalanceAsync();
        balance!.Received.Should().Be(5);
        balance.Returned.Should().Be(5);
        balance.Outstanding.Should().Be(0);
    }

    [Fact]
    public async Task Refill_PartialEmpties_TracksOutstanding()
    {
        await SeedAsync();
        await SellAsync(_refillId, 5, emptiesReturned: 3);

        var balance = await BalanceAsync();
        balance!.Outstanding.Should().Be(2); // 5 received, 3 returned
    }

    [Fact]
    public async Task PackageSale_DoesNotTouchLedger()
    {
        await SeedAsync();
        await SellAsync(_packageId, 3, emptiesReturned: null);

        (await BalanceAsync()).Should().BeNull(); // ownership transferred, nothing owed
    }

    [Fact]
    public async Task AdvanceRefill_OneCall_CreatesCreditSale_Stock_AndCylinderDue()
    {
        await SeedAsync(stock: 50);

        using (var c = NewContext())
        {
            var result = await AdvanceSvc(c).CreateAsync(new CreateAdvanceRefillRequest
            {
                CustomerId = _customerId,
                WarehouseId = _warehouseId,
                ProductId = _refillId,
                Quantity = 4,
            });
            result.IsSuccess.Should().BeTrue(result.Error);
            result.Data!.Outstanding.Should().Be(4);
        }

        using var check = NewContext();
        var order = await check.SalesOrders.SingleAsync();
        order.Status.Should().Be(SalesOrderStatus.Delivered);
        order.IsCreditSale.Should().BeTrue();
        order.TotalAmount.Should().Be(6000m); // 4 x 1500

        (await check.StockLevels.FirstAsync(s => s.ProductId == _refillId)).Quantity.Should().Be(46); // stock moved
        (await BalanceAsync())!.Outstanding.Should().Be(4); // cylinders owed
    }

    [Fact]
    public async Task AdvanceRefill_FailsWithoutStock_NoSideEffects()
    {
        await SeedAsync(stock: 2);

        using var c = NewContext();
        var result = await AdvanceSvc(c).CreateAsync(new CreateAdvanceRefillRequest
        {
            CustomerId = _customerId, WarehouseId = _warehouseId, ProductId = _refillId, Quantity = 10,
        });

        result.IsSuccess.Should().BeFalse();
        (await BalanceAsync()).Should().BeNull(); // no cylinder liability recorded
    }

    [Fact]
    public async Task Return_ReducesOutstanding_AndOverReturnRejected()
    {
        await SeedAsync();
        await SellAsync(_refillId, 5, emptiesReturned: 0, credit: true); // 5 outstanding

        using (var c = NewContext())
        {
            var ok = await LedgerSvc(c).AdjustBalanceAsync(new AdjustCylinderBalanceRequest
            {
                CustomerId = _customerId, BrandId = _brandId, CylinderSizeId = _sizeId, Quantity = 3, IsReturn = true,
            });
            ok.IsSuccess.Should().BeTrue(ok.Error);
        }
        (await BalanceAsync())!.Outstanding.Should().Be(2);

        using (var c = NewContext())
        {
            var over = await LedgerSvc(c).AdjustBalanceAsync(new AdjustCylinderBalanceRequest
            {
                CustomerId = _customerId, BrandId = _brandId, CylinderSizeId = _sizeId, Quantity = 5, IsReturn = true,
            });
            over.IsSuccess.Should().BeFalse(); // only 2 outstanding
        }
        (await BalanceAsync())!.Outstanding.Should().Be(2);
    }

    [Fact]
    public async Task ReturnWithSettlement_CreatesPaymentAgainstCreditOrder()
    {
        await SeedAsync();
        var orderId = await SellAsync(_refillId, 5, emptiesReturned: 0, credit: true); // due 7500

        using (var c = NewContext())
        {
            var result = await LedgerSvc(c).AdjustBalanceAsync(new AdjustCylinderBalanceRequest
            {
                CustomerId = _customerId, BrandId = _brandId, CylinderSizeId = _sizeId,
                Quantity = 5, IsReturn = true,
                SettlementSalesOrderId = orderId, SettlementAmount = 2000m,
            });
            result.IsSuccess.Should().BeTrue(result.Error);
        }

        using var check = NewContext();
        var payment = await check.Payments.SingleAsync(p => p.SalesOrderId == orderId);
        payment.Amount.Should().Be(2000m);
        payment.Direction.Should().Be(PaymentDirection.Inbound);
        payment.Reference.Should().Be("CYL-RETURN");
        (await BalanceAsync())!.Outstanding.Should().Be(0);
    }

    [Fact]
    public async Task Settlement_ExceedingOrderDue_Rejected()
    {
        await SeedAsync();
        var orderId = await SellAsync(_refillId, 2, emptiesReturned: 0, credit: true); // due 3000

        using var c = NewContext();
        var result = await LedgerSvc(c).AdjustBalanceAsync(new AdjustCylinderBalanceRequest
        {
            CustomerId = _customerId, BrandId = _brandId, CylinderSizeId = _sizeId,
            Quantity = 2, IsReturn = true,
            SettlementSalesOrderId = orderId, SettlementAmount = 5000m,
        });

        result.IsSuccess.Should().BeFalse();
        (await c.Payments.CountAsync()).Should().Be(0);
    }
}
