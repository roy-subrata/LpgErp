using AutoMapper;
using FluentAssertions;
using LpgErp.Application.Common.Mappings;
using LpgErp.Application.Features.PurchaseOrders;
using LpgErp.Application.Features.PurchaseOrders.DTOs;
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
/// The cylinder side of buying from the company: empties go back one-for-one against refills,
/// and leaking cylinders are settled by a free refill, a credit, or a replacement.
/// </summary>
public class CylinderReturnTests
{
    private readonly InMemoryDatabaseRoot _root = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()), NullLoggerFactory.Instance).CreateMapper();

    private LpgErpDbContext NewContext() =>
        new(new DbContextOptionsBuilder<LpgErpDbContext>()
            .UseInMemoryDatabase("cylinder-returns", _root)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private PurchaseOrderService NewService(LpgErpDbContext c) => new(c, new UnitOfWork(c), _mapper);

    private Guid _supplierId, _warehouseId, _brandId, _sizeId, _emptyId, _refillId;

    private async Task SeedAsync(int emptyStock = 0, int damagedStock = 0)
    {
        using var c = NewContext();
        var supplier = new Supplier { Name = "Bashundhara" };
        var warehouse = new Warehouse { Name = "Main" };
        var brand = new Brand { Name = "Bashundhara LPG" };
        var size = new CylinderSize { Name = "12 KG" };
        c.AddRange(supplier, warehouse, brand, size);
        await c.SaveChangesAsync();

        var empty = new Product
        {
            Name = "12KG Empty", Type = ProductType.EmptyCylinder,
            BrandId = brand.Id, CylinderSizeId = size.Id,
            CurrentStock = emptyStock, DamagedStock = damagedStock,
        };
        var refill = new Product
        {
            Name = "12KG Refill", Type = ProductType.GasRefill,
            BrandId = brand.Id, CylinderSizeId = size.Id,
        };
        c.Products.AddRange(empty, refill);
        await c.SaveChangesAsync();

        c.StockLevels.Add(new StockLevel
        {
            WarehouseId = warehouse.Id, ProductId = empty.Id,
            Quantity = emptyStock, DamagedQuantity = damagedStock,
        });
        await c.SaveChangesAsync();

        _supplierId = supplier.Id;
        _warehouseId = warehouse.Id;
        _brandId = brand.Id;
        _sizeId = size.Id;
        _emptyId = empty.Id;
        _refillId = refill.Id;
    }

    private CreatePurchaseOrderRequest RefillOrder(int qty, List<CreateLeakageRequest>? leakages = null) => new()
    {
        SupplierId = _supplierId,
        WarehouseId = _warehouseId,
        Items = [new CreatePurchaseOrderItemRequest { ProductId = _refillId, OrderedQuantity = qty, UnitPrice = 1000m }],
        Leakages = leakages ?? [],
    };

    private async Task<Guid> CreateAndConfirmAsync(CreatePurchaseOrderRequest request)
    {
        Guid id;
        using (var c = NewContext())
        {
            var created = await NewService(c).CreateAsync(request);
            created.IsSuccess.Should().BeTrue(created.Error);
            id = created.Data!.Id;
        }
        using (var c = NewContext()) await NewService(c).ConfirmAsync(id);
        return id;
    }

    private async Task<(int Good, int Damaged)> StockAsync()
    {
        using var c = NewContext();
        var level = await c.StockLevels.FirstAsync(s => s.ProductId == _emptyId);
        return (level.Quantity, level.DamagedQuantity);
    }

    [Fact]
    public async Task Buying_refills_sends_one_empty_per_refill_back_to_the_company()
    {
        await SeedAsync(emptyStock: 100);
        var orderId = await CreateAndConfirmAsync(RefillOrder(40));

        using (var c = NewContext())
        {
            var result = await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
            {
                Items = [new ReceiveItemRequest { ProductId = _refillId, ReceivedQuantity = 40 }]
            });
            result.IsSuccess.Should().BeTrue(result.Error);
        }

        // 40 refills in means 40 empties went back — this used to never happen, so empty stock only grew.
        (await StockAsync()).Good.Should().Be(60);
    }

    [Fact]
    public async Task Sending_fewer_empties_than_refills_leaves_the_rest_owed()
    {
        await SeedAsync(emptyStock: 100);
        var orderId = await CreateAndConfirmAsync(RefillOrder(40));

        using (var c = NewContext())
            await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
            {
                Items = [new ReceiveItemRequest { ProductId = _refillId, ReceivedQuantity = 40, EmptySentQuantity = 30 }]
            });

        (await StockAsync()).Good.Should().Be(70);

        using var check = NewContext();
        var item = await check.PurchaseOrderItems.FirstAsync(i => i.PurchaseOrderId == orderId);
        item.EmptySentQuantity.Should().Be(30);
        item.EmptyOwedQuantity.Should().Be(10); // ten cylinders still owed to the company
    }

    [Fact]
    public async Task Claiming_to_send_more_empties_than_are_held_is_rejected()
    {
        await SeedAsync(emptyStock: 5);
        var orderId = await CreateAndConfirmAsync(RefillOrder(40));

        using var c = NewContext();
        var result = await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
        {
            Items = [new ReceiveItemRequest { ProductId = _refillId, ReceivedQuantity = 40, EmptySentQuantity = 20 }]
        });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Only 5 empty");
    }

    [Fact]
    public async Task Leakage_settled_by_credit_reduces_what_is_owed_and_returns_no_goods()
    {
        await SeedAsync(emptyStock: 100, damagedStock: 10);
        var orderId = await CreateAndConfirmAsync(RefillOrder(40, [
            new CreateLeakageRequest
            {
                BrandId = _brandId, CylinderSizeId = _sizeId, Quantity = 10,
                Resolution = LeakageResolution.CreditAdjustment, CreditAmount = 3000m,
            }
        ]));

        Guid leakageId;
        using (var c = NewContext())
            leakageId = (await c.PurchaseOrderLeakages.FirstAsync(l => l.PurchaseOrderId == orderId)).Id;

        using (var c = NewContext())
        {
            var result = await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
            {
                Items = [new ReceiveItemRequest { ProductId = _refillId, ReceivedQuantity = 40 }],
                Leakages = [new SettleLeakageRequest { LeakageId = leakageId, SettledQuantity = 10 }],
            });
            result.IsSuccess.Should().BeTrue(result.Error);
        }

        var stock = await StockAsync();
        stock.Damaged.Should().Be(0);  // the leaking cylinders physically went back
        stock.Good.Should().Be(60);    // 100 - 40 empties for refills; a credit returns no cylinders

        using var check = NewContext();
        var order = await check.PurchaseOrders.FirstAsync(o => o.Id == orderId);
        order.LeakageCredit.Should().Be(3000m);
        order.NetPayable.Should().Be(40 * 1000m - 3000m);
    }

    [Fact]
    public async Task Leakage_settled_by_free_refill_returns_gas_without_charge()
    {
        await SeedAsync(emptyStock: 100, damagedStock: 6);
        var orderId = await CreateAndConfirmAsync(RefillOrder(10, [
            new CreateLeakageRequest
            {
                BrandId = _brandId, CylinderSizeId = _sizeId, Quantity = 6,
                Resolution = LeakageResolution.FreeRefill,
            }
        ]));

        Guid leakageId;
        using (var c = NewContext())
            leakageId = (await c.PurchaseOrderLeakages.FirstAsync(l => l.PurchaseOrderId == orderId)).Id;

        using (var c = NewContext())
        {
            var result = await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
            {
                Items = [new ReceiveItemRequest { ProductId = _refillId, ReceivedQuantity = 10 }],
                Leakages = [new SettleLeakageRequest { LeakageId = leakageId, SettledQuantity = 6 }],
            });
            result.IsSuccess.Should().BeTrue(result.Error);
        }

        using var check = NewContext();
        var refillStock = await check.StockLevels.FirstAsync(s => s.ProductId == _refillId);
        refillStock.Quantity.Should().Be(16);  // 10 bought + 6 refilled free

        var order = await check.PurchaseOrders.FirstAsync(o => o.Id == orderId);
        order.LeakageCredit.Should().Be(0m);   // settled in goods, not money
        order.NetPayable.Should().Be(10 * 1000m);

        (await StockAsync()).Damaged.Should().Be(0);
    }

    [Fact]
    public async Task Leakage_settled_by_replacement_returns_good_empties()
    {
        await SeedAsync(emptyStock: 100, damagedStock: 8);
        var orderId = await CreateAndConfirmAsync(RefillOrder(10, [
            new CreateLeakageRequest
            {
                BrandId = _brandId, CylinderSizeId = _sizeId, Quantity = 8,
                Resolution = LeakageResolution.Replacement,
            }
        ]));

        Guid leakageId;
        using (var c = NewContext())
            leakageId = (await c.PurchaseOrderLeakages.FirstAsync(l => l.PurchaseOrderId == orderId)).Id;

        using (var c = NewContext())
            await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
            {
                Items = [new ReceiveItemRequest { ProductId = _refillId, ReceivedQuantity = 10 }],
                Leakages = [new SettleLeakageRequest { LeakageId = leakageId, SettledQuantity = 8 }],
            });

        var stock = await StockAsync();
        stock.Damaged.Should().Be(0);
        // 100 - 10 sent for refills + 8 good replacements back.
        stock.Good.Should().Be(98);
    }

    [Fact]
    public async Task Returning_more_leaking_cylinders_than_are_held_is_rejected()
    {
        await SeedAsync(emptyStock: 50, damagedStock: 3);

        using var c = NewContext();
        var result = await NewService(c).CreateAsync(RefillOrder(10, [
            new CreateLeakageRequest
            {
                BrandId = _brandId, CylinderSizeId = _sizeId, Quantity = 9,
                Resolution = LeakageResolution.Replacement,
            }
        ]));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Only 3 damaged");
    }

    [Fact]
    public async Task A_credit_adjustment_needs_an_amount()
    {
        await SeedAsync(emptyStock: 50, damagedStock: 5);

        using var c = NewContext();
        var result = await NewService(c).CreateAsync(RefillOrder(10, [
            new CreateLeakageRequest
            {
                BrandId = _brandId, CylinderSizeId = _sizeId, Quantity = 5,
                Resolution = LeakageResolution.CreditAdjustment, CreditAmount = 0m,
            }
        ]));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("amount the company is taking off");
    }
}
