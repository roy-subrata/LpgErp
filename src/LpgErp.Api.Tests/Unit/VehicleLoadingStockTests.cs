using AutoMapper;
using FluentAssertions;
using LpgErp.Application.Common.Mappings;
using LpgErp.Application.Features.VehicleLoadings;
using LpgErp.Application.Features.VehicleLoadings.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Infrastructure.Persistence;
using LpgErp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace LpgErp.Api.Tests.Unit;

public class VehicleLoadingStockTests
{
    private readonly InMemoryDatabaseRoot _root = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile())).CreateMapper();

    private LpgErpDbContext NewContext() =>
        new(new DbContextOptionsBuilder<LpgErpDbContext>()
            .UseInMemoryDatabase("vehicle-loading", _root)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private VehicleLoadingService NewService(LpgErpDbContext context) =>
        new(context, new UnitOfWork(context), _mapper);

    private Guid _warehouseId, _truckId, _driverId, _salesmanId, _productId;

    private async Task SeedAsync(int warehouseStock)
    {
        using var context = NewContext();
        var warehouse = new Warehouse { Name = "Main" };
        var truck = new Truck { RegistrationNumber = "DH-1234" };
        var driver = new Driver { Name = "Karim" };
        var salesman = new Salesman { Name = "Arif" };
        var product = new Product { Name = "12KG Package", Type = ProductType.NewPackage, CurrentStock = warehouseStock };

        context.Warehouses.Add(warehouse);
        context.Trucks.Add(truck);
        context.Drivers.Add(driver);
        context.Salesmen.Add(salesman);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        context.StockLevels.Add(new StockLevel { WarehouseId = warehouse.Id, ProductId = product.Id, Quantity = warehouseStock });
        await context.SaveChangesAsync();

        _warehouseId = warehouse.Id;
        _truckId = truck.Id;
        _driverId = driver.Id;
        _salesmanId = salesman.Id;
        _productId = product.Id;
    }

    private CreateVehicleLoadingRequest LoadingRequest(int qty) => new()
    {
        LoadingDate = DateTime.UtcNow,
        TruckId = _truckId,
        DriverId = _driverId,
        SalesmanId = _salesmanId,
        WarehouseId = _warehouseId,
        Items = [new CreateVehicleLoadingItemRequest { ProductId = _productId, LoadedQuantity = qty }]
    };

    private async Task<int> WarehouseStockAsync()
    {
        using var c = NewContext();
        return (await c.StockLevels.FirstAsync(s => s.WarehouseId == _warehouseId && s.ProductId == _productId)).Quantity;
    }

    [Fact]
    public async Task Create_DeductsWarehouseStock_AndWritesMovement()
    {
        await SeedAsync(warehouseStock: 100);

        Guid loadingId;
        using (var c = NewContext())
            loadingId = (await NewService(c).CreateAsync(LoadingRequest(40))).Data!.Id;

        (await WarehouseStockAsync()).Should().Be(60);

        using var check = NewContext();
        var movement = await check.StockMovements.SingleAsync(m => m.ProductId == _productId);
        movement.Type.Should().Be(StockMovementType.TransferOut);
        movement.Quantity.Should().Be(40);
        movement.FromWarehouseId.Should().Be(_warehouseId);
    }

    [Fact]
    public async Task Create_FailsWhenWarehouseStockInsufficient()
    {
        await SeedAsync(warehouseStock: 10);

        using var c = NewContext();
        var result = await NewService(c).CreateAsync(LoadingRequest(40));

        result.IsSuccess.Should().BeFalse();
        (await WarehouseStockAsync()).Should().Be(10); // untouched
    }

    [Fact]
    public async Task Update_AdjustsStockByDelta()
    {
        await SeedAsync(warehouseStock: 100);

        Guid loadingId;
        using (var c = NewContext())
            loadingId = (await NewService(c).CreateAsync(LoadingRequest(40))).Data!.Id; // stock 60

        using (var c = NewContext())
        {
            var update = new UpdateVehicleLoadingRequest
            {
                LoadingDate = DateTime.UtcNow,
                TruckId = _truckId,
                DriverId = _driverId,
                SalesmanId = _salesmanId,
                WarehouseId = _warehouseId,
                Items = [new CreateVehicleLoadingItemRequest { ProductId = _productId, LoadedQuantity = 25 }]
            };
            (await NewService(c).UpdateAsync(loadingId, update)).IsSuccess.Should().BeTrue();
        }

        // 40 loaded -> 25 loaded: 15 returned to the warehouse.
        (await WarehouseStockAsync()).Should().Be(75);
    }

    [Fact]
    public async Task Close_ReturnsUnsold_AndDeductsSoldAndDamagedFromCompanyStock()
    {
        await SeedAsync(warehouseStock: 100);

        Guid loadingId;
        using (var c = NewContext())
            loadingId = (await NewService(c).CreateAsync(LoadingRequest(50))).Data!.Id; // warehouse 50

        using (var c = NewContext())
        {
            var close = new CreateVehicleClosingRequest
            {
                VehicleLoadingId = loadingId,
                CashCollected = 30000m,
                Items =
                [
                    // 50 loaded: 35 sold, 12 returned, 2 damaged (1 unaccounted = variance).
                    new CreateVehicleClosingItemRequest { ProductId = _productId, LoadedQuantity = 50, SoldQuantity = 35, ReturnedQuantity = 12, DamagedQuantity = 2 }
                ]
            };
            (await NewService(c).CloseAsync(loadingId, close)).IsSuccess.Should().BeTrue();
        }

        (await WarehouseStockAsync()).Should().Be(62); // 50 after loading + 12 returned

        using var check = NewContext();
        (await check.Products.FindAsync(_productId))!.CurrentStock.Should().Be(63); // 100 - 35 sold - 2 damaged
        (await check.VehicleLoadings.FindAsync(loadingId))!.Status.Should().Be(VehicleLoadingStatus.Returned);
        (await check.StockMovements.CountAsync(m => m.Type == StockMovementType.SaleOut)).Should().Be(1);
        (await check.StockMovements.CountAsync(m => m.Type == StockMovementType.Return)).Should().Be(1);
        (await check.StockMovements.CountAsync(m => m.Type == StockMovementType.Adjustment)).Should().Be(1);
    }

    [Fact]
    public async Task Close_RejectsSecondClosing_AndOverReporting()
    {
        await SeedAsync(warehouseStock: 100);

        Guid loadingId;
        using (var c = NewContext())
            loadingId = (await NewService(c).CreateAsync(LoadingRequest(50))).Data!.Id;

        // Over-reporting: sold + returned + damaged > loaded.
        using (var c = NewContext())
        {
            var bad = new CreateVehicleClosingRequest
            {
                VehicleLoadingId = loadingId,
                Items = [new CreateVehicleClosingItemRequest { ProductId = _productId, LoadedQuantity = 50, SoldQuantity = 40, ReturnedQuantity = 15, DamagedQuantity = 0 }]
            };
            (await NewService(c).CloseAsync(loadingId, bad)).IsSuccess.Should().BeFalse();
        }

        using (var c = NewContext())
        {
            var ok = new CreateVehicleClosingRequest
            {
                VehicleLoadingId = loadingId,
                Items = [new CreateVehicleClosingItemRequest { ProductId = _productId, LoadedQuantity = 50, SoldQuantity = 40, ReturnedQuantity = 10, DamagedQuantity = 0 }]
            };
            (await NewService(c).CloseAsync(loadingId, ok)).IsSuccess.Should().BeTrue();
        }

        // Second closing must be rejected.
        using (var c = NewContext())
        {
            var again = new CreateVehicleClosingRequest
            {
                VehicleLoadingId = loadingId,
                Items = [new CreateVehicleClosingItemRequest { ProductId = _productId, LoadedQuantity = 50, SoldQuantity = 0, ReturnedQuantity = 50, DamagedQuantity = 0 }]
            };
            var result = await NewService(c).CloseAsync(loadingId, again);
            result.IsSuccess.Should().BeFalse();
        }

        (await WarehouseStockAsync()).Should().Be(60); // 50 + 10 returned, unchanged by rejected attempts
    }

    [Fact]
    public async Task Delete_DispatchedLoading_RestoresWarehouseStock()
    {
        await SeedAsync(warehouseStock: 100);

        Guid loadingId;
        using (var c = NewContext())
            loadingId = (await NewService(c).CreateAsync(LoadingRequest(40))).Data!.Id; // stock 60

        using (var c = NewContext())
            (await NewService(c).DeleteAsync(loadingId)).IsSuccess.Should().BeTrue();

        (await WarehouseStockAsync()).Should().Be(100);
    }
}
