using AutoMapper;
using FluentAssertions;
using LpgErp.Application.Common.Mappings;
using LpgErp.Application.Features.SalesOrders;
using LpgErp.Application.Features.SalesOrders.DTOs;
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

/// <summary>
/// Proves total inventory stays correct when vehicle sales are recorded as sales orders,
/// as closing-only quantities, or a mix — the same sale is never deducted twice.
/// </summary>
public class VehicleSalesInventoryTests
{
    private readonly InMemoryDatabaseRoot _root = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile())).CreateMapper();

    private LpgErpDbContext NewContext() =>
        new(new DbContextOptionsBuilder<LpgErpDbContext>()
            .UseInMemoryDatabase("vehicle-sales", _root)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private Guid _warehouseId, _truckId, _driverId, _salesmanId, _customerId, _productId;

    private async Task SeedAsync(int stock)
    {
        using var context = NewContext();
        var warehouse = new Warehouse { Name = "Main" };
        var truck = new Truck { Name = "T1" };
        var driver = new Driver { Name = "Karim" };
        var salesman = new Salesman { Name = "Arif" };
        var customer = new Customer { Name = "Hotel Star" };
        var product = new Product { Name = "12KG Package", Type = ProductType.NewPackage, CurrentStock = stock };

        context.Warehouses.Add(warehouse);
        context.Trucks.Add(truck);
        context.Drivers.Add(driver);
        context.Salesmen.Add(salesman);
        context.Customers.Add(customer);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        context.StockLevels.Add(new StockLevel { WarehouseId = warehouse.Id, ProductId = product.Id, Quantity = stock });
        await context.SaveChangesAsync();

        _warehouseId = warehouse.Id;
        _truckId = truck.Id;
        _driverId = driver.Id;
        _salesmanId = salesman.Id;
        _customerId = customer.Id;
        _productId = product.Id;
    }

    private async Task<Guid> DispatchAsync(int qty)
    {
        using var c = NewContext();
        var service = new VehicleLoadingService(c, new UnitOfWork(c), _mapper);
        var result = await service.CreateAsync(new CreateVehicleLoadingRequest
        {
            LoadingDate = DateTime.UtcNow,
            TruckId = _truckId,
            DriverId = _driverId,
            SalesmanId = _salesmanId,
            WarehouseId = _warehouseId,
            Items = [new CreateVehicleLoadingItemRequest { ProductId = _productId, LoadedQuantity = qty }]
        });
        return result.Data!.Id;
    }

    private async Task<(bool ok, string? error, Guid orderId)> SellFromVehicleAsync(Guid loadingId, int qty)
    {
        using var c = NewContext();
        var service = new SalesOrderService(c, new UnitOfWork(c), _mapper);
        var created = await service.CreateAsync(new CreateSalesOrderRequest
        {
            CustomerId = _customerId,
            WarehouseId = _warehouseId,
            VehicleLoadingId = loadingId,
            Items = [new CreateSalesOrderItemRequest { ProductId = _productId, Quantity = qty, UnitPrice = 1500m }]
        });
        await service.ConfirmAsync(created.Data!.Id);
        var delivered = await service.DeliverAsync(created.Data.Id);
        return (delivered.IsSuccess, delivered.Error, created.Data.Id);
    }

    private async Task<(int warehouse, int current)> StockAsync()
    {
        using var c = NewContext();
        var level = await c.StockLevels.FirstAsync(s => s.WarehouseId == _warehouseId && s.ProductId == _productId);
        var product = await c.Products.FindAsync(_productId);
        return (level.Quantity, product!.CurrentStock);
    }

    [Fact]
    public async Task VehicleSale_DeductsVehicleStock_NotWarehouse()
    {
        await SeedAsync(stock: 100);
        var loadingId = await DispatchAsync(50); // warehouse 50, current 100

        var (ok, error, _) = await SellFromVehicleAsync(loadingId, 20);

        ok.Should().BeTrue(error);
        var (warehouse, current) = await StockAsync();
        warehouse.Should().Be(50);  // untouched by the vehicle sale
        current.Should().Be(80);    // 100 - 20 sold
    }

    [Fact]
    public async Task VehicleSale_CannotExceedLoadedStock()
    {
        await SeedAsync(stock: 100);
        var loadingId = await DispatchAsync(30);

        (await SellFromVehicleAsync(loadingId, 20)).ok.Should().BeTrue();
        var (ok, _, _) = await SellFromVehicleAsync(loadingId, 15); // only 10 left on the truck

        ok.Should().BeFalse();
        (await StockAsync()).current.Should().Be(80); // only the first sale deducted
    }

    [Fact]
    public async Task VehicleSale_RejectedAfterVehicleClosed()
    {
        await SeedAsync(stock: 100);
        var loadingId = await DispatchAsync(30);

        using (var c = NewContext())
        {
            var service = new VehicleLoadingService(c, new UnitOfWork(c), _mapper);
            (await service.CloseAsync(loadingId, new CreateVehicleClosingRequest
            {
                VehicleLoadingId = loadingId,
                Items = [new CreateVehicleClosingItemRequest { ProductId = _productId, LoadedQuantity = 30, SoldQuantity = 0, ReturnedQuantity = 30, DamagedQuantity = 0 }]
            })).IsSuccess.Should().BeTrue();
        }

        var (ok, _, _) = await SellFromVehicleAsync(loadingId, 5);
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task Closing_DoesNotDoubleCount_OrderRecordedSales()
    {
        await SeedAsync(stock: 100);
        var loadingId = await DispatchAsync(50); // warehouse 50, current 100

        // 20 sold via a recorded sales order (deducted at delivery), 10 more sold as unrecorded cash sales.
        (await SellFromVehicleAsync(loadingId, 20)).ok.Should().BeTrue(); // current 80

        using (var c = NewContext())
        {
            var service = new VehicleLoadingService(c, new UnitOfWork(c), _mapper);
            var result = await service.CloseAsync(loadingId, new CreateVehicleClosingRequest
            {
                VehicleLoadingId = loadingId,
                // Total sold for the day = 30 (20 recorded + 10 cash), 20 returned.
                Items = [new CreateVehicleClosingItemRequest { ProductId = _productId, LoadedQuantity = 50, SoldQuantity = 30, ReturnedQuantity = 20, DamagedQuantity = 0 }]
            });
            result.IsSuccess.Should().BeTrue(result.Error);
        }

        var (warehouse, current) = await StockAsync();
        warehouse.Should().Be(70);  // 50 + 20 returned
        current.Should().Be(70);    // 100 - 30 total sold, NOT 100 - 50 (no double deduction)
    }

    [Fact]
    public async Task Closing_RejectsSoldLessThanRecordedOrders()
    {
        await SeedAsync(stock: 100);
        var loadingId = await DispatchAsync(50);

        (await SellFromVehicleAsync(loadingId, 20)).ok.Should().BeTrue();

        using var c = NewContext();
        var service = new VehicleLoadingService(c, new UnitOfWork(c), _mapper);
        var result = await service.CloseAsync(loadingId, new CreateVehicleClosingRequest
        {
            VehicleLoadingId = loadingId,
            // Claims only 10 sold but 20 were already sold via recorded orders — inconsistent.
            Items = [new CreateVehicleClosingItemRequest { ProductId = _productId, LoadedQuantity = 50, SoldQuantity = 10, ReturnedQuantity = 40, DamagedQuantity = 0 }]
        });

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DirectWarehouseSale_StillDeductsWarehouseStock()
    {
        await SeedAsync(stock: 100);

        using var c = NewContext();
        var service = new SalesOrderService(c, new UnitOfWork(c), _mapper);
        var created = await service.CreateAsync(new CreateSalesOrderRequest
        {
            CustomerId = _customerId,
            WarehouseId = _warehouseId,
            Items = [new CreateSalesOrderItemRequest { ProductId = _productId, Quantity = 10, UnitPrice = 1500m }]
        });
        await service.ConfirmAsync(created.Data!.Id);
        (await service.DeliverAsync(created.Data.Id)).IsSuccess.Should().BeTrue();

        var (warehouse, current) = await StockAsync();
        warehouse.Should().Be(90);
        current.Should().Be(90);
    }
}
