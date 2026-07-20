using LpgErp.Application.Common.Interfaces;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using LpgErp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Infrastructure.Persistence;

public class LpgErpDbContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    private readonly ICurrentUserService? _currentUserService;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

    public LpgErpDbContext(DbContextOptions<LpgErpDbContext> options, ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<CylinderSize> CylinderSizes => Set<CylinderSize>();
    public DbSet<Cylinder> Cylinders => Set<Cylinder>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<CustomerCylinderBalance> CustomerCylinderBalances => Set<CustomerCylinderBalance>();
    public DbSet<Truck> Trucks => Set<Truck>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Salesman> Salesmen => Set<Salesman>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<VehicleLoading> VehicleLoadings => Set<VehicleLoading>();
    public DbSet<VehicleLoadingItem> VehicleLoadingItems => Set<VehicleLoadingItem>();
    public DbSet<VehicleClosing> VehicleClosings => Set<VehicleClosing>();
    public DbSet<VehicleClosingItem> VehicleClosingItems => Set<VehicleClosingItem>();
    public DbSet<DailySalesSummary> DailySalesSummaries => Set<DailySalesSummary>();
    public DbSet<DriverSettlement> DriverSettlements => Set<DriverSettlement>();
    public DbSet<SalesmanSettlement> SalesmanSettlements => Set<SalesmanSettlement>();
    public DbSet<CylinderDeposit> CylinderDeposits => Set<CylinderDeposit>();
    public DbSet<CylinderExchange> CylinderExchanges => Set<CylinderExchange>();
    public DbSet<CustomerNotification> CustomerNotifications => Set<CustomerNotification>();
    public DbSet<TransportCompany> TransportCompanies => Set<TransportCompany>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LpgErpDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUserService?.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUserService?.UserId;
                    break;
                case EntityState.Deleted:
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = _currentUserService?.UserId;
                    entry.State = EntityState.Modified;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
