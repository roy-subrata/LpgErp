using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Infrastructure.Persistence;

/// <summary>
/// Seeds baseline master data for local/development use. Each section is
/// idempotent: it only runs when the target table is empty, so it is safe to
/// call on every startup.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(LpgErpDbContext db, CancellationToken ct = default)
    {
        var brands = await SeedBrandsAsync(db, ct);
        var warehouses = await SeedWarehousesAsync(db, ct);
        var sizes = await SeedCylinderSizesAsync(db, brands, ct);
        await SeedProductsAsync(db, brands, sizes, ct);
        await SeedSuppliersAsync(db, brands, ct);
        await SeedCustomersAsync(db, ct);
        await SeedRoutesAsync(db, ct);
        await SeedDriversAsync(db, ct);
        await SeedSalesmenAsync(db, ct);
        await SeedTrucksAsync(db, ct);
        await SeedTransportCompaniesAsync(db, ct);
        await SeedStockLevelsAsync(db, ct);
        await SeedTransactionsAsync(db, ct);
    }

    /// <summary>
    /// Places each product's CurrentStock into the first (main) warehouse so warehouse-level
    /// stock matches company totals and vehicle loadings/sales have stock to draw from.
    /// </summary>
    private static async Task SeedStockLevelsAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.StockLevels.AnyAsync(ct)) return;

        var mainWarehouse = await db.Warehouses.OrderBy(w => w.CreatedAt).FirstOrDefaultAsync(ct);
        if (mainWarehouse is null) return;

        var products = await db.Products.Where(p => !p.IsDeleted && p.CurrentStock > 0).ToListAsync(ct);
        foreach (var product in products)
        {
            db.StockLevels.Add(new StockLevel
            {
                WarehouseId = mainWarehouse.Id,
                ProductId = product.Id,
                Quantity = product.CurrentStock
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task<Dictionary<string, Brand>> SeedBrandsAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.Brands.AnyAsync(ct))
            return await db.Brands.ToDictionaryAsync(b => b.Name, ct);

        var brands = new List<Brand>
        {
            new() { Id = Guid.NewGuid(), Name = "Bashundhara", Code = "BSH", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Omera", Code = "OMR", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "BM", Code = "BM", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Petromax", Code = "PMX", IsActive = true },
        };
        db.Brands.AddRange(brands);
        await db.SaveChangesAsync(ct);
        return brands.ToDictionary(b => b.Name);
    }

    private static async Task<List<Warehouse>> SeedWarehousesAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.Warehouses.AnyAsync(ct))
            return await db.Warehouses.ToListAsync(ct);

        var warehouses = new List<Warehouse>
        {
            new() { Id = Guid.NewGuid(), Name = "Main Depot", Code = "WH-01", Address = "Tejgaon I/A, Dhaka", Phone = "02-9110001", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Narayanganj Store", Code = "WH-02", Address = "Fatullah, Narayanganj", Phone = "02-7660002", IsActive = true },
        };
        db.Warehouses.AddRange(warehouses);
        await db.SaveChangesAsync(ct);
        return warehouses;
    }

    private static async Task<List<CylinderSize>> SeedCylinderSizesAsync(LpgErpDbContext db, Dictionary<string, Brand> brands, CancellationToken ct)
    {
        if (await db.CylinderSizes.AnyAsync(ct))
            return await db.CylinderSizes.ToListAsync(ct);

        var sizes = new List<CylinderSize>();
        var specs = new (decimal weight, decimal deposit)[] { (12m, 1500m), (35m, 3500m), (45m, 4200m) };
        foreach (var brand in brands.Values)
        {
            foreach (var (weight, deposit) in specs)
            {
                sizes.Add(new CylinderSize
                {
                    Id = Guid.NewGuid(),
                    BrandId = brand.Id,
                    Name = $"{weight:0}kg",
                    WeightKg = weight,
                    DepositAmount = deposit,
                    IsActive = true,
                });
            }
        }
        db.CylinderSizes.AddRange(sizes);
        await db.SaveChangesAsync(ct);
        return sizes;
    }

    private static async Task SeedProductsAsync(LpgErpDbContext db, Dictionary<string, Brand> brands, List<CylinderSize> sizes, CancellationToken ct)
    {
        if (await db.Products.AnyAsync(ct))
            return;

        var products = new List<Product>();
        foreach (var brand in brands.Values)
        {
            var brandSizes = sizes.Where(s => s.BrandId == brand.Id).ToList();
            foreach (var size in brandSizes)
            {
                var basePrice = size.WeightKg == 12m ? 1200m : size.WeightKg == 35m ? 3200m : 4000m;
                products.Add(new Product
                {
                    Id = Guid.NewGuid(), Name = $"{brand.Name} {size.Name} Refill", Code = $"{brand.Code}-{size.WeightKg:0}-R",
                    Type = ProductType.GasRefill, BrandId = brand.Id, CylinderSizeId = size.Id,
                    PurchasePrice = basePrice * 0.85m, SalePrice = basePrice, CurrentStock = 120, MinimumStock = 30, IsActive = true,
                });
                products.Add(new Product
                {
                    Id = Guid.NewGuid(), Name = $"{brand.Name} {size.Name} Package", Code = $"{brand.Code}-{size.WeightKg:0}-P",
                    Type = ProductType.NewPackage, BrandId = brand.Id, CylinderSizeId = size.Id,
                    PurchasePrice = (basePrice + size.DepositAmount) * 0.9m, SalePrice = basePrice + size.DepositAmount,
                    CurrentStock = 40, MinimumStock = 10, IsActive = true,
                });
            }
        }
        products.Add(new Product { Id = Guid.NewGuid(), Name = "Regulator (22mm)", Code = "ACC-REG", Type = ProductType.Accessory, PurchasePrice = 220m, SalePrice = 350m, CurrentStock = 200, MinimumStock = 50, IsActive = true });
        products.Add(new Product { Id = Guid.NewGuid(), Name = "Gas Hose (1.5m)", Code = "ACC-HOSE", Type = ProductType.Accessory, PurchasePrice = 90m, SalePrice = 160m, CurrentStock = 300, MinimumStock = 60, IsActive = true });

        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedSuppliersAsync(LpgErpDbContext db, Dictionary<string, Brand> brands, CancellationToken ct)
    {
        if (await db.Suppliers.AnyAsync(ct))
            return;

        var suppliers = new List<Supplier>
        {
            new() { Id = Guid.NewGuid(), Name = "Bashundhara LP Gas Ltd", Code = "SUP-BSH", ContactPerson = "Rezaul Karim", Phone = "01711100001", IsLpgCompany = true, BrandId = brands["Bashundhara"].Id, CommissionPerCylinder = 50m, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Omera Petroleum Ltd", Code = "SUP-OMR", ContactPerson = "Nasrin Akter", Phone = "01711100002", IsLpgCompany = true, BrandId = brands["Omera"].Id, CommissionPerCylinder = 40m, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Beximco LPG", Code = "SUP-BM", ContactPerson = "Kamrul Hasan", Phone = "01711100003", IsLpgCompany = true, BrandId = brands["BM"].Id, CommissionPerCylinder = 35m, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "United Accessories", Code = "SUP-ACC", ContactPerson = "Sajib Roy", Phone = "01711100004", IsLpgCompany = false, IsActive = true },
        };
        db.Suppliers.AddRange(suppliers);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedCustomersAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.Customers.AnyAsync(ct))
            return;

        var customers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), Name = "Rahim General Store", Code = "CUS-0001", Type = CustomerType.Retail, ContactPerson = "Abdur Rahim", Phone = "01811000001", Address = "Mirpur 10, Dhaka", CreditLimit = 20000m, PaymentDueDays = 15, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Karim Traders", Code = "CUS-0002", Type = CustomerType.WholesaleDealer, ContactPerson = "Abdul Karim", Phone = "01811000002", Address = "Savar, Dhaka", CreditLimit = 150000m, PaymentDueDays = 30, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Star Kabab & Restaurant", Code = "CUS-0003", Type = CustomerType.Restaurant, ContactPerson = "Jahangir Alam", Phone = "01811000003", Address = "Dhanmondi, Dhaka", CreditLimit = 80000m, PaymentDueDays = 20, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Hotel Sonargaon Service", Code = "CUS-0004", Type = CustomerType.Hotel, ContactPerson = "Farhana Yasmin", Phone = "01811000004", Address = "Panthapath, Dhaka", CreditLimit = 200000m, PaymentDueDays = 30, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Delta Textiles", Code = "CUS-0005", Type = CustomerType.Industrial, ContactPerson = "Mizanur Rahman", Phone = "01811000005", Address = "Ashulia, Dhaka", CreditLimit = 500000m, PaymentDueDays = 45, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Nazma Stores", Code = "CUS-0006", Type = CustomerType.Retail, ContactPerson = "Nazma Begum", Phone = "01811000006", Address = "Uttara, Dhaka", CreditLimit = 15000m, PaymentDueDays = 15, IsActive = true },
        };
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedRoutesAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.Routes.AnyAsync(ct))
            return;

        var routes = new List<Route>
        {
            new() { Id = Guid.NewGuid(), Name = "Mirpur Circuit", Area = "Mirpur", Village = "Mirpur 1-14", Dealer = "Rahim General Store", Description = "North Dhaka retail loop", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Savar Line", Area = "Savar", Village = "Savar, Ashulia", Dealer = "Karim Traders", Description = "Industrial belt", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Dhanmondi Route", Area = "Dhanmondi", Village = "Dhanmondi, Panthapath", Dealer = "Star Kabab", Description = "Restaurant & hotel corridor", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Narayanganj Route", Area = "Narayanganj", Village = "Fatullah", Dealer = "Delta Textiles", Description = "Narayanganj distribution", IsActive = true },
        };
        db.Routes.AddRange(routes);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedDriversAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.Drivers.AnyAsync(ct))
            return;

        var drivers = new List<Driver>
        {
            new() { Id = Guid.NewGuid(), Name = "Sohel Rana", Phone = "01911000001", LicenseNumber = "DL-DHK-114521", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Jamal Uddin", Phone = "01911000002", LicenseNumber = "DL-DHK-118834", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Babul Miah", Phone = "01911000003", LicenseNumber = "DL-DHK-121097", IsActive = true },
        };
        db.Drivers.AddRange(drivers);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedSalesmenAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.Salesmen.AnyAsync(ct))
            return;

        var salesmen = new List<Salesman>
        {
            new() { Id = Guid.NewGuid(), Name = "Arif Hossain", Phone = "01611000001", EmployeeCode = "SM-001", DailyCommissionRate = 2.5m, DailyAllowance = 400m, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Rubel Ahmed", Phone = "01611000002", EmployeeCode = "SM-002", DailyCommissionRate = 2.0m, DailyAllowance = 400m, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Shakil Khan", Phone = "01611000003", EmployeeCode = "SM-003", DailyCommissionRate = 3.0m, DailyAllowance = 450m, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Tanvir Islam", Phone = "01611000004", EmployeeCode = "SM-004", DailyCommissionRate = 2.5m, DailyAllowance = 400m, IsActive = true },
        };
        db.Salesmen.AddRange(salesmen);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedTrucksAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.Trucks.AnyAsync(ct))
            return;

        var trucks = new List<Truck>
        {
            new() { Id = Guid.NewGuid(), Name = "Truck A", RegistrationNumber = "DHK-METRO-KHA-1122", Phone = "01911000001", FuelCapacity = 120m, CurrentMileage = 84210m, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Truck B", RegistrationNumber = "DHK-METRO-GHA-3344", Phone = "01911000002", FuelCapacity = 120m, CurrentMileage = 61540m, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Truck C", RegistrationNumber = "DHK-METRO-KHA-5566", Phone = "01911000003", FuelCapacity = 90m, CurrentMileage = 33980m, IsActive = true },
        };
        db.Trucks.AddRange(trucks);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedTransportCompaniesAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.TransportCompanies.AnyAsync(ct))
            return;

        var companies = new List<TransportCompany>
        {
            new() { Id = Guid.NewGuid(), Name = "Padma Carriers", ContactPerson = "Selim Reza", Phone = "01511000001", Address = "Kamalapur, Dhaka", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Meghna Logistics", ContactPerson = "Habibur Rahman", Phone = "01511000002", Address = "Postogola, Dhaka", IsActive = true },
        };
        db.TransportCompanies.AddRange(companies);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedTransactionsAsync(LpgErpDbContext db, CancellationToken ct)
    {
        if (await db.SalesOrders.AnyAsync(ct))
            return;

        var customers = await db.Customers.Where(c => !c.IsDeleted).ToListAsync(ct);
        var warehouses = await db.Warehouses.Where(w => !w.IsDeleted).ToListAsync(ct);
        var products = await db.Products.Where(p => !p.IsDeleted).ToListAsync(ct);
        var routes = await db.Routes.Where(r => !r.IsDeleted).ToListAsync(ct);
        var drivers = await db.Drivers.Where(d => !d.IsDeleted).ToListAsync(ct);
        var salesmen = await db.Salesmen.Where(s => !s.IsDeleted).ToListAsync(ct);
        var trucks = await db.Trucks.Where(t => !t.IsDeleted).ToListAsync(ct);

        if (customers.Count == 0 || warehouses.Count == 0 || products.Count == 0)
            return;

        var sellable = products.Where(p => p.Type == ProductType.GasRefill).ToList();
        if (sellable.Count == 0) sellable = products;
        var wh = warehouses[0];
        var rnd = new Random(42);
        var today = DateTime.UtcNow.Date;

        // ---- Sales orders + payments (last 14 days) ----
        var salesOrders = new List<SalesOrder>();
        var payments = new List<Payment>();
        var seq = 1;
        for (var d = 13; d >= 0; d--)
        {
            var date = today.AddDays(-d);
            var ordersToday = 2 + (d % 3);
            for (var o = 0; o < ordersToday; o++)
            {
                var customer = customers[rnd.Next(customers.Count)];
                var lineCount = 1 + rnd.Next(2);
                var items = new List<SalesOrderItem>();
                decimal total = 0;
                for (var li = 0; li < lineCount; li++)
                {
                    var product = sellable[rnd.Next(sellable.Count)];
                    var qty = 3 + rnd.Next(18);
                    items.Add(new SalesOrderItem { Id = Guid.NewGuid(), ProductId = product.Id, Quantity = qty, UnitPrice = product.SalePrice });
                    total += qty * product.SalePrice;
                }

                var isCredit = rnd.Next(100) < 35;
                var discount = rnd.Next(100) < 20 ? Math.Round(total * 0.03m, 0) : 0m;
                var so = new SalesOrder
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = $"SO-{seq:D5}",
                    CustomerId = customer.Id,
                    WarehouseId = wh.Id,
                    Status = isCredit ? SalesOrderStatus.Confirmed : SalesOrderStatus.Delivered,
                    TotalAmount = total,
                    Discount = discount,
                    OrderDate = date.AddHours(9 + o),
                    IsCreditSale = isCredit,
                    DueDate = isCredit ? date.AddDays(customer.PaymentDueDays) : null,
                    RouteId = routes.Count > 0 ? routes[rnd.Next(routes.Count)].Id : null,
                    Items = items,
                };
                salesOrders.Add(so);
                seq++;

                var net = total - discount;
                if (!isCredit)
                {
                    payments.Add(new Payment { Id = Guid.NewGuid(), SalesOrderId = so.Id, Method = PaymentMethod.Cash, Direction = PaymentDirection.Inbound, Amount = net, PaymentDate = so.OrderDate, Reference = $"RCP-{seq:D5}" });
                }
                else if (rnd.Next(100) < 60)
                {
                    var paid = Math.Round(net * 0.5m, 0);
                    var method = rnd.Next(2) == 0 ? PaymentMethod.MobileBanking : PaymentMethod.Bank;
                    payments.Add(new Payment { Id = Guid.NewGuid(), SalesOrderId = so.Id, Method = method, Direction = PaymentDirection.Inbound, Amount = paid, PaymentDate = so.OrderDate.AddDays(2), Reference = $"RCP-{seq:D5}" });
                }
            }
        }
        db.SalesOrders.AddRange(salesOrders);
        db.Payments.AddRange(payments);
        await db.SaveChangesAsync(ct);

        if (drivers.Count == 0 || salesmen.Count == 0 || trucks.Count == 0)
            return;

        // ---- Vehicle loadings + closings + driver settlements (last 10 days) ----
        var loadings = new List<VehicleLoading>();
        var closings = new List<VehicleClosing>();
        var driverSettlements = new List<DriverSettlement>();
        var loadSeq = 0;
        for (var d = 9; d >= 0; d--)
        {
            var date = today.AddDays(-d);
            var truck = trucks[loadSeq % trucks.Count];
            var driver = drivers[loadSeq % drivers.Count];
            var salesman = salesmen[loadSeq % salesmen.Count];
            var route = routes.Count > 0 ? routes[loadSeq % routes.Count] : null;
            var returned = d > 0;

            var loadItems = new List<VehicleLoadingItem>();
            foreach (var product in sellable.Take(3))
            {
                var loaded = 30 + rnd.Next(30);
                var sold = returned ? (int)(loaded * (0.6 + rnd.NextDouble() * 0.3)) : 0;
                var damaged = returned && rnd.Next(100) < 20 ? rnd.Next(2) : 0;
                var ret = returned ? loaded - sold - damaged : 0;
                loadItems.Add(new VehicleLoadingItem { Id = Guid.NewGuid(), ProductId = product.Id, LoadedQuantity = loaded, SoldQuantity = sold, ReturnedQuantity = ret, DamagedQuantity = damaged, Price = (int)product.SalePrice });
            }

            var loading = new VehicleLoading
            {
                Id = Guid.NewGuid(),
                LoadingDate = date.AddHours(7),
                TruckId = truck.Id,
                DriverId = driver.Id,
                SalesmanId = salesman.Id,
                WarehouseId = wh.Id,
                RouteId = route?.Id,
                Status = returned ? VehicleLoadingStatus.Returned : VehicleLoadingStatus.Dispatched,
                Items = loadItems,
            };
            loadings.Add(loading);

            if (returned)
            {
                decimal grossSales = 0;
                var cylReturned = 0;
                var damagedTotal = 0;
                var closingItems = new List<VehicleClosingItem>();
                foreach (var li in loadItems)
                {
                    var prod = sellable.First(p => p.Id == li.ProductId);
                    grossSales += li.SoldQuantity * prod.SalePrice;
                    cylReturned += li.ReturnedQuantity;
                    damagedTotal += li.DamagedQuantity;
                    closingItems.Add(new VehicleClosingItem { Id = Guid.NewGuid(), ProductId = li.ProductId, LoadedQuantity = li.LoadedQuantity, SoldQuantity = li.SoldQuantity, ReturnedQuantity = li.ReturnedQuantity, DamagedQuantity = li.DamagedQuantity });
                }
                var credit = Math.Round(grossSales * 0.2m, 0);
                closings.Add(new VehicleClosing
                {
                    Id = Guid.NewGuid(),
                    VehicleLoadingId = loading.Id,
                    ClosingDate = date.AddHours(18),
                    CashCollected = grossSales - credit,
                    CreditSales = credit,
                    OutstandingAmount = credit,
                    CylinderExchanges = rnd.Next(5, 20),
                    ReturnedEmptyCylinders = cylReturned,
                    DamagedCount = damagedTotal,
                    LeakageCount = 0,
                    Variance = 0,
                    Items = closingItems,
                });

                driverSettlements.Add(new DriverSettlement
                {
                    Id = Guid.NewGuid(),
                    DriverId = driver.Id,
                    SettlementDate = date.AddHours(19),
                    VehicleLoadingId = loading.Id,
                    TripCount = 1,
                    FuelCost = 800 + rnd.Next(400),
                    Allowance = 300,
                    LoadingCost = 150,
                    UnloadingCost = 150,
                    TripIncome = 1500 + rnd.Next(500),
                    CompanyPickupIncentive = rnd.Next(100) < 40 ? 500 : 0,
                    Distance = 40 + rnd.Next(60),
                });
            }
            loadSeq++;
        }
        db.VehicleLoadings.AddRange(loadings);
        db.VehicleClosings.AddRange(closings);
        db.DriverSettlements.AddRange(driverSettlements);
        await db.SaveChangesAsync(ct);

        // ---- Salesman settlements (per salesman, last 3 days) ----
        var salesmanSettlements = new List<SalesmanSettlement>();
        foreach (var sm in salesmen)
        {
            for (var d = 2; d >= 0; d--)
            {
                var date = today.AddDays(-d);
                decimal totalSales = 8000 + rnd.Next(12000);
                salesmanSettlements.Add(new SalesmanSettlement
                {
                    Id = Guid.NewGuid(),
                    SalesmanId = sm.Id,
                    SettlementDate = date.AddHours(19),
                    TotalSales = totalSales,
                    Collection = Math.Round(totalSales * 0.85m, 0),
                    Commission = Math.Round(totalSales * sm.DailyCommissionRate / 100m, 0),
                    DailyAllowance = sm.DailyAllowance,
                    Bonus = rnd.Next(100) < 30 ? 500 : 0,
                });
            }
        }
        db.SalesmanSettlements.AddRange(salesmanSettlements);
        await db.SaveChangesAsync(ct);
    }
}
