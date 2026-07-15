using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Brand> Brands { get; }
    DbSet<CylinderSize> CylinderSizes { get; }
    DbSet<Cylinder> Cylinders { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Product> Products { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderItem> SalesOrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<StockLevel> StockLevels { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<CustomerCylinderBalance> CustomerCylinderBalances { get; }
    DbSet<Truck> Trucks { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<Salesman> Salesmen { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
