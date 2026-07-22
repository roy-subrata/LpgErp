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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace LpgErp.Api.Tests.Unit;

public class PurchaseOrderCommissionTests
{
    // Shared root so every context in a test sees the same in-memory store; a new context
    // per operation mirrors the app's scoped-per-request DbContext lifecycle.
    private readonly InMemoryDatabaseRoot _root = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()), NullLoggerFactory.Instance).CreateMapper();

    private LpgErpDbContext NewContext() =>
        new(new DbContextOptionsBuilder<LpgErpDbContext>()
            .UseInMemoryDatabase("commission", _root)
            // InMemory has no transactions; ignore the warning so BeginTransactionAsync is a no-op.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private PurchaseOrderService NewService(LpgErpDbContext context) =>
        new(context, new UnitOfWork(context), _mapper);

    private Guid _supplierId, _warehouseId, _cylinderId, _gasId;

    private async Task SeedAsync(decimal commissionPerCylinder)
    {
        using var context = NewContext();
        var supplier = new Supplier { Name = "Bashundhara", CommissionPerCylinder = commissionPerCylinder };
        var warehouse = new Warehouse { Name = "Main" };
        var cylinder = new Product { Name = "12KG Empty Cylinder", Type = ProductType.EmptyCylinder, PurchasePrice = 800m };
        var gas = new Product { Name = "12KG Gas Refill", Type = ProductType.GasRefill, PurchasePrice = 1000m };

        context.Suppliers.Add(supplier);
        context.Warehouses.Add(warehouse);
        context.Products.AddRange(cylinder, gas);
        await context.SaveChangesAsync();

        _supplierId = supplier.Id;
        _warehouseId = warehouse.Id;
        _cylinderId = cylinder.Id;
        _gasId = gas.Id;
    }

    private CreatePurchaseOrderRequest OrderFor(Guid productId, int qty, decimal unitPrice) => new()
    {
        SupplierId = _supplierId,
        WarehouseId = _warehouseId,
        Items = [new CreatePurchaseOrderItemRequest { ProductId = productId, OrderedQuantity = qty, UnitPrice = unitPrice }]
    };

    // Runs create -> confirm -> receive (all good, no damage) for a single-product order, each on its own context.
    private async Task<Guid> ReceiveOrderAsync(Guid productId, int qty, decimal unitPrice)
    {
        Guid orderId;
        using (var c = NewContext()) orderId = (await NewService(c).CreateAsync(OrderFor(productId, qty, unitPrice))).Data!.Id;
        using (var c = NewContext()) await NewService(c).ConfirmAsync(orderId);
        using (var c = NewContext())
            await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
            {
                Items = [new ReceiveItemRequest { ProductId = productId, ReceivedQuantity = qty, DamagedQuantity = 0 }]
            });
        return orderId;
    }

    private async Task<decimal> SupplierBalanceAsync()
    {
        using var c = NewContext();
        return (await c.Suppliers.FindAsync(_supplierId))!.CommissionBalance;
    }

    [Fact]
    public async Task Receive_AccruesCommission_OnlyForCylinderProducts()
    {
        await SeedAsync(commissionPerCylinder: 50m);

        var create = OrderFor(_cylinderId, 100, 800m);
        create.Items.Add(new CreatePurchaseOrderItemRequest { ProductId = _gasId, OrderedQuantity = 100, UnitPrice = 1000m });

        Guid orderId;
        using (var c = NewContext()) orderId = (await NewService(c).CreateAsync(create)).Data!.Id;
        using (var c = NewContext()) await NewService(c).ConfirmAsync(orderId);
        using (var c = NewContext())
            await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
            {
                Items =
                [
                    new ReceiveItemRequest { ProductId = _cylinderId, ReceivedQuantity = 100, DamagedQuantity = 0 },
                    new ReceiveItemRequest { ProductId = _gasId, ReceivedQuantity = 100, DamagedQuantity = 0 },
                ]
            });

        // 100 cylinders * 50 = 5000 (gas refills do not carry a cylinder, so no accrual).
        (await SupplierBalanceAsync()).Should().Be(5000m);
    }

    [Fact]
    public async Task Receive_DamagedCylinders_DoNotAccrueCommission()
    {
        await SeedAsync(commissionPerCylinder: 50m);

        Guid orderId;
        using (var c = NewContext()) orderId = (await NewService(c).CreateAsync(OrderFor(_cylinderId, 100, 800m))).Data!.Id;
        using (var c = NewContext()) await NewService(c).ConfirmAsync(orderId);
        using (var c = NewContext())
            await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
            {
                Items = [new ReceiveItemRequest { ProductId = _cylinderId, ReceivedQuantity = 100, DamagedQuantity = 10 }]
            });

        // Only 90 good cylinders * 50 = 4500.
        (await SupplierBalanceAsync()).Should().Be(4500m);
    }

    [Fact]
    public async Task Create_AutoAppliesCommissionBalance_AgainstNextOrder()
    {
        await SeedAsync(commissionPerCylinder: 50m);
        await ReceiveOrderAsync(_cylinderId, 100, 800m); // -> 5000 balance

        // Second order: 10 cylinders * 800 = 8000 payable; 5000 balance fully applied.
        using var c = NewContext();
        var second = await NewService(c).CreateAsync(OrderFor(_cylinderId, 10, 800m));

        second.Data!.CommissionApplied.Should().Be(5000m);
        second.Data.NetPayable.Should().Be(3000m); // 8000 - 5000
        (await SupplierBalanceAsync()).Should().Be(0m);
    }

    [Fact]
    public async Task Create_AppliesCappedAtPayable_WhenBalanceExceedsOrder()
    {
        await SeedAsync(commissionPerCylinder: 50m);
        await ReceiveOrderAsync(_cylinderId, 100, 800m); // -> 5000 balance

        using var c = NewContext();
        var second = await NewService(c).CreateAsync(OrderFor(_cylinderId, 1, 800m));

        second.Data!.CommissionApplied.Should().Be(800m); // capped at the 800 payable
        second.Data.NetPayable.Should().Be(0m);
        (await SupplierBalanceAsync()).Should().Be(4200m); // 5000 - 800
    }

    [Fact]
    public async Task Delete_DraftOrder_RefundsAppliedCommission()
    {
        await SeedAsync(commissionPerCylinder: 50m);
        await ReceiveOrderAsync(_cylinderId, 100, 800m); // -> 5000 balance

        Guid secondId;
        using (var c = NewContext())
        {
            var second = await NewService(c).CreateAsync(OrderFor(_cylinderId, 1, 800m));
            second.Data!.CommissionApplied.Should().Be(800m);
            secondId = second.Data.Id;
        }
        (await SupplierBalanceAsync()).Should().Be(4200m);

        using (var c = NewContext()) await NewService(c).DeleteAsync(secondId);

        // Applied commission returns to the balance.
        (await SupplierBalanceAsync()).Should().Be(5000m);
    }

    [Fact]
    public async Task Receive_ShortDelivery_RecordsMissingAndStaysPartiallyReceived()
    {
        await SeedAsync(commissionPerCylinder: 50m);

        Guid orderId;
        using (var c = NewContext()) orderId = (await NewService(c).CreateAsync(OrderFor(_cylinderId, 100, 800m))).Data!.Id;
        using (var c = NewContext()) await NewService(c).ConfirmAsync(orderId);

        PurchaseOrderDto result;
        using (var c = NewContext())
            result = (await NewService(c).ReceiveAsync(orderId, new ReceivePurchaseOrderRequest
            {
                // 90 arrive, 5 damaged, 10 reported missing/lost in transit (short delivery).
                Items = [new ReceiveItemRequest { ProductId = _cylinderId, ReceivedQuantity = 90, DamagedQuantity = 5, MissingQuantity = 10 }]
            })).Data!;

        var item = result.Items.Single();
        item.ReceivedQuantity.Should().Be(90);
        item.DamagedQuantity.Should().Be(5);
        item.MissingQuantity.Should().Be(10);
        item.ShortQuantity.Should().Be(10);            // 100 ordered - 90 received
        result.Status.Should().Be(PurchaseOrderStatus.PartiallyReceived);

        // Only good units (90 - 5 = 85) accrue commission: 85 * 50 = 4250.
        (await SupplierBalanceAsync()).Should().Be(4250m);
    }

    [Fact]
    public async Task NoCommissionRate_LeavesBalanceZero()
    {
        await SeedAsync(commissionPerCylinder: 0m);

        using var c = NewContext();
        var created = await NewService(c).CreateAsync(OrderFor(_cylinderId, 100, 800m));
        await NewService(c).ConfirmAsync(created.Data!.Id);

        created.Data.CommissionApplied.Should().Be(0m);
        (await SupplierBalanceAsync()).Should().Be(0m);
    }
}
