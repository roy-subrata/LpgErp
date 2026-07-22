using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Reports.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.Reports;

public interface IReportService
{
    Task<Result<IReadOnlyList<InventoryReportDto>>> GetInventoryReportAsync(Guid? warehouseId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<BrandInventoryDto>>> GetBrandInventoryAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<LowStockAlertDto>>> GetLowStockAlertsAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<CylinderMovementDto>>> GetCylinderMovementHistoryAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<SalesReportDto>>> GetSalesReportAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CustomerSalesSummaryDto>>> GetSalesByCustomerAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProductTypeSalesDto>>> GetProductTypeSalesAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PurchaseReportDto>>> GetPurchaseReportAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CustomerReportDto>>> GetCustomerReportAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<VehicleLoadingReportDto>>> GetVehicleLoadingReportAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<FinancialReportDto>> GetFinancialReportAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<RouteSalesDto>>> GetSalesByRouteAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<BrandSalesDto>>> GetBrandWiseSalesAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VehicleReconciliationDto>>> GetVehicleReconciliationAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<DriverProductivityDto>>> GetDriverProductivityAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<SalesmanProductivityDto>>> GetSalesmanProductivityAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CylinderSizeSalesDto>>> GetCylinderSizeSalesAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<AdvanceRefillReportDto>>> GetAdvanceRefillReportAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<RefillHistoryDto>>> GetRefillHistoryAsync(Guid customerId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VehicleSalesDto>>> GetSalesByVehicleAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CreditPurchaseDto>>> GetCreditPurchasesAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<CashFlowEntryDto>>> GetCashFlowAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PnLCategoryDto>>> GetPnLBreakdownAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;

    public ReportService(IApplicationDbContext context) { _context = context; }

    public async Task<Result<IReadOnlyList<InventoryReportDto>>> GetInventoryReportAsync(Guid? warehouseId, CancellationToken ct = default)
    {
        var query = _context.StockLevels
            .Where(s => !s.IsDeleted)
            .Include(s => s.Product).ThenInclude(p => p.Brand)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == warehouseId.Value);

        var items = await query.ToListAsync(ct);
        var report = items.Select(s => new InventoryReportDto
        {
            ProductName = s.Product.Name,
            WarehouseName = s.Warehouse.Name,
            Quantity = s.Quantity,
            BrandName = s.Product.Brand?.Name ?? "",
            ProductType = s.Product.Type.ToString()
        }).ToList();

        return Result<IReadOnlyList<InventoryReportDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<BrandInventoryDto>>> GetBrandInventoryAsync(CancellationToken ct = default)
    {
        var stockLevels = await _context.StockLevels
            .Where(s => !s.IsDeleted)
            .Include(s => s.Product).ThenInclude(p => p.Brand)
            .ToListAsync(ct);

        var report = stockLevels
            .GroupBy(s => s.Product.Brand?.Name ?? "Unknown")
            .Select(g => new BrandInventoryDto
            {
                BrandName = g.Key,
                EmptyCylinderCount = g.Where(s => s.Product.Type == ProductType.EmptyCylinder).Sum(s => s.Quantity),
                FilledCylinderCount = g.Where(s => s.Product.Type == ProductType.NewPackage || s.Product.Type == ProductType.GasRefill).Sum(s => s.Quantity),
                TotalQuantity = g.Sum(s => s.Quantity)
            })
            .OrderBy(b => b.BrandName)
            .ToList();

        return Result<IReadOnlyList<BrandInventoryDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<LowStockAlertDto>>> GetLowStockAlertsAsync(CancellationToken ct = default)
    {
        var products = await _context.Products
            .Where(p => !p.IsDeleted && p.IsActive)
            .Include(p => p.Brand)
            .ToListAsync(ct);

        var stockLevels = await _context.StockLevels
            .Where(s => !s.IsDeleted)
            .Include(s => s.Warehouse)
            .ToListAsync(ct);

        var alerts = new List<LowStockAlertDto>();
        foreach (var product in products)
        {
            var totalStock = stockLevels.Where(s => s.ProductId == product.Id).Sum(s => s.Quantity);
            if (totalStock < product.MinimumStock)
            {
                alerts.Add(new LowStockAlertDto
                {
                    ProductName = product.Name,
                    BrandName = product.Brand?.Name ?? "",
                    WarehouseName = "All Warehouses",
                    CurrentStock = totalStock,
                    MinimumStock = product.MinimumStock,
                    Deficit = product.MinimumStock - totalStock
                });
            }
        }

        return Result<IReadOnlyList<LowStockAlertDto>>.Success(alerts.OrderByDescending(a => a.Deficit).ToList());
    }

    public async Task<Result<IReadOnlyList<CylinderMovementDto>>> GetCylinderMovementHistoryAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var movements = await _context.StockMovements
            .Where(m => !m.IsDeleted && m.MovementDate >= from && m.MovementDate <= to)
            .Include(m => m.Product)
            .Include(m => m.FromWarehouse)
            .Include(m => m.ToWarehouse)
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync(ct);

        var report = movements.Select(m => new CylinderMovementDto
        {
            Date = m.MovementDate,
            ProductName = m.Product.Name,
            Type = m.Type.ToString(),
            Quantity = m.Quantity,
            FromWarehouse = m.FromWarehouse?.Name,
            ToWarehouse = m.ToWarehouse?.Name,
            Reference = m.Reference
        }).ToList();

        return Result<IReadOnlyList<CylinderMovementDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<SalesReportDto>>> GetSalesReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var items = await _context.SalesOrders
            .Where(so => !so.IsDeleted && so.OrderDate >= from && so.OrderDate <= to)
            .Include(so => so.Customer)
            .OrderByDescending(so => so.OrderDate)
            .ToListAsync(ct);

        var report = items.Select(so => new SalesReportDto
        {
            OrderNumber = so.OrderNumber,
            CustomerName = so.Customer.Name,
            OrderDate = so.OrderDate,
            TotalAmount = so.TotalAmount,
            Discount = so.Discount,
            NetAmount = so.NetAmount,
            Status = so.Status.ToString()
        }).ToList();

        return Result<IReadOnlyList<SalesReportDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<CustomerSalesSummaryDto>>> GetSalesByCustomerAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var orders = await _context.SalesOrders
            .Where(so => !so.IsDeleted && so.OrderDate >= from && so.OrderDate <= to)
            .Include(so => so.Customer)
            .ToListAsync(ct);

        var orderIds = orders.Select(so => so.Id).ToList();
        var payments = await _context.Payments
            .Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Inbound
                && p.SalesOrderId != null && orderIds.Contains(p.SalesOrderId.Value))
            .ToListAsync(ct);

        var report = orders
            .GroupBy(so => so.Customer.Name)
            .Select(g => new CustomerSalesSummaryDto
            {
                CustomerName = g.Key,
                OrderCount = g.Count(),
                TotalPurchases = g.Sum(so => so.NetAmount),
                TotalPayments = payments.Where(p => g.Any(so => so.Id == p.SalesOrderId)).Sum(p => p.Amount),
                Outstanding = g.Sum(so => so.NetAmount) - payments.Where(p => g.Any(so => so.Id == p.SalesOrderId)).Sum(p => p.Amount)
            })
            .OrderByDescending(c => c.TotalPurchases)
            .ToList();

        return Result<IReadOnlyList<CustomerSalesSummaryDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<ProductTypeSalesDto>>> GetProductTypeSalesAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var items = await _context.SalesOrderItems
            .Where(i => !i.IsDeleted)
            .Include(i => i.SalesOrder)
            .Include(i => i.Product)
            .Where(i => i.SalesOrder!.OrderDate >= from && i.SalesOrder.OrderDate <= to && !i.SalesOrder.IsDeleted)
            .ToListAsync(ct);

        var report = items
            .GroupBy(i => i.Product.Type.ToString())
            .Select(g => new ProductTypeSalesDto
            {
                ProductType = g.Key,
                QuantitySold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .ToList();

        return Result<IReadOnlyList<ProductTypeSalesDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<PurchaseReportDto>>> GetPurchaseReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var items = await _context.PurchaseOrders
            .Where(po => !po.IsDeleted && po.OrderDate >= from && po.OrderDate <= to)
            .Include(po => po.Supplier)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync(ct);

        var report = items.Select(po => new PurchaseReportDto
        {
            OrderNumber = po.OrderNumber,
            SupplierName = po.Supplier.Name,
            OrderDate = po.OrderDate,
            TotalAmount = po.TotalAmount,
            CommissionEarned = po.CommissionEarned,
            Status = po.Status.ToString()
        }).ToList();

        return Result<IReadOnlyList<PurchaseReportDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<CustomerReportDto>>> GetCustomerReportAsync(CancellationToken ct = default)
    {
        var customers = await _context.Customers.Where(c => !c.IsDeleted).ToListAsync(ct);
        var report = new List<CustomerReportDto>();

        foreach (var customer in customers)
        {
            var totalPurchases = await _context.SalesOrders.Where(so => so.CustomerId == customer.Id && !so.IsDeleted).SumAsync(so => so.TotalAmount - so.Discount, ct);
            var totalPayments = await _context.Payments.Where(p => p.SalesOrderId != null && !p.IsDeleted && _context.SalesOrders.Any(so => so.Id == p.SalesOrderId && so.CustomerId == customer.Id)).SumAsync(p => p.Amount, ct);
            var cylinderBalance = await _context.CustomerCylinderBalances.Where(c => c.CustomerId == customer.Id && !c.IsDeleted).SumAsync(c => c.Received - c.Returned, ct);

            report.Add(new CustomerReportDto
            {
                CustomerName = customer.Name,
                CustomerCode = customer.Code ?? "",
                CreditLimit = customer.CreditLimit,
                TotalPurchases = totalPurchases,
                TotalPayments = totalPayments,
                OutstandingBalance = totalPurchases - totalPayments,
                CylinderBalance = cylinderBalance
            });
        }

        return Result<IReadOnlyList<CustomerReportDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<VehicleLoadingReportDto>>> GetVehicleLoadingReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var loadings = await _context.VehicleLoadings
            .Where(v => !v.IsDeleted && v.LoadingDate >= from && v.LoadingDate <= to)
            .Include(v => v.Truck)
            .Include(v => v.Driver)
            .Include(v => v.Salesman)
            .Include(v => v.Warehouse)
            .Include(v => v.Items)
            .OrderByDescending(v => v.LoadingDate)
            .ToListAsync(ct);

        var report = loadings.Select(v => new VehicleLoadingReportDto
        {
            Date = v.LoadingDate,
            TruckName = v.Truck.Name,
            DriverName = v.Driver.Name,
            SalesmanName = v.Salesman.Name,
            WarehouseName = v.Warehouse.Name,
            Status = v.Status.ToString(),
            ItemCount = v.Items.Count
        }).ToList();

        return Result<IReadOnlyList<VehicleLoadingReportDto>>.Success(report);
    }

    public async Task<Result<FinancialReportDto>> GetFinancialReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var sales = await _context.SalesOrders.Where(so => !so.IsDeleted && so.OrderDate >= from && so.OrderDate <= to).SumAsync(so => so.TotalAmount - so.Discount, ct);
        var payments = await _context.Payments.Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Inbound && p.PaymentDate >= from && p.PaymentDate <= to).SumAsync(p => p.Amount, ct);
        var purchases = await _context.PurchaseOrders.Where(po => !po.IsDeleted && po.OrderDate >= from && po.OrderDate <= to).SumAsync(po => po.TotalAmount, ct);
        var purchasePayments = await _context.Payments.Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Outbound && p.PaymentDate >= from && p.PaymentDate <= to).SumAsync(p => p.Amount, ct);
        var deposits = await _context.CylinderDeposits.Where(d => !d.IsDeleted).SumAsync(d => d.Amount, ct);

        var settlements = await _context.DriverSettlements
            .Where(s => !s.IsDeleted && s.SettlementDate >= from && s.SettlementDate <= to)
            .ToListAsync(ct);
        var transportExpenses = settlements.Sum(s => s.FuelCost + s.LoadingCost + s.UnloadingCost);

        var commissionEarned = await _context.PurchaseOrders
            .Where(po => !po.IsDeleted && po.OrderDate >= from && po.OrderDate <= to)
            .SumAsync(po => po.CommissionEarned, ct);

        var report = new FinancialReportDto
        {
            TotalSales = sales,
            TotalPayments = payments,
            TotalPurchases = purchases,
            TotalPurchasePayments = purchasePayments,
            AccountsReceivable = sales - payments,
            SupplierPayable = purchases - purchasePayments,
            TransportationExpenses = transportExpenses,
            CommissionBalance = commissionEarned,
            DepositLiability = deposits
        };

        return Result<FinancialReportDto>.Success(report);
    }

    public async Task<Result<IReadOnlyList<RouteSalesDto>>> GetSalesByRouteAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var orders = await _context.SalesOrders
            .Where(so => !so.IsDeleted && so.OrderDate >= from && so.OrderDate <= to)
            .Include(so => so.Route)
            .ToListAsync(ct);

        var orderIds = orders.Select(so => so.Id).ToList();
        var payments = await _context.Payments
            .Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Inbound
                && p.SalesOrderId != null && orderIds.Contains(p.SalesOrderId.Value))
            .ToListAsync(ct);

        var report = orders
            .GroupBy(so => so.Route?.Name ?? "No Route")
            .Select(g => new RouteSalesDto
            {
                RouteName = g.Key,
                OrderCount = g.Count(),
                TotalSales = g.Sum(so => so.NetAmount),
                TotalPayments = payments.Where(p => g.Any(so => so.Id == p.SalesOrderId)).Sum(p => p.Amount),
                Outstanding = g.Sum(so => so.NetAmount) - payments.Where(p => g.Any(so => so.Id == p.SalesOrderId)).Sum(p => p.Amount)
            })
            .OrderByDescending(r => r.TotalSales)
            .ToList();

        return Result<IReadOnlyList<RouteSalesDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<BrandSalesDto>>> GetBrandWiseSalesAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var items = await _context.SalesOrderItems
            .Where(i => !i.IsDeleted)
            .Include(i => i.SalesOrder)
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Where(i => i.SalesOrder!.OrderDate >= from && i.SalesOrder.OrderDate <= to && !i.SalesOrder.IsDeleted)
            .ToListAsync(ct);

        var report = items
            .GroupBy(i => i.Product.Brand?.Name ?? "Unknown")
            .Select(g => new BrandSalesDto
            {
                BrandName = g.Key,
                QuantitySold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(b => b.TotalRevenue)
            .ToList();

        return Result<IReadOnlyList<BrandSalesDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<VehicleReconciliationDto>>> GetVehicleReconciliationAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var closings = await _context.VehicleClosings
            .Where(vc => !vc.IsDeleted && vc.ClosingDate >= from && vc.ClosingDate <= to)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Truck)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Driver)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Salesman)
            .Include(vc => vc.Items)
            .OrderByDescending(vc => vc.ClosingDate)
            .ToListAsync(ct);

        var report = closings.Select(vc => new VehicleReconciliationDto
        {
            VehicleLoadingId = vc.VehicleLoadingId,
            Date = vc.ClosingDate,
            TruckName = vc.VehicleLoading.Truck.Name,
            DriverName = vc.VehicleLoading.Driver.Name,
            SalesmanName = vc.VehicleLoading.Salesman.Name,
            TotalLoaded = vc.Items.Sum(i => i.LoadedQuantity),
            TotalSold = vc.Items.Sum(i => i.SoldQuantity),
            TotalReturned = vc.Items.Sum(i => i.ReturnedQuantity),
            TotalDamaged = vc.Items.Sum(i => i.DamagedQuantity),
            Variance = vc.Variance,
            CashCollected = vc.CashCollected,
            CreditSales = vc.CreditSales
        }).ToList();

        return Result<IReadOnlyList<VehicleReconciliationDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<DriverProductivityDto>>> GetDriverProductivityAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var settlements = await _context.DriverSettlements
            .Where(s => !s.IsDeleted && s.SettlementDate >= from && s.SettlementDate <= to)
            .Include(s => s.Driver)
            .ToListAsync(ct);

        var report = settlements
            .GroupBy(s => s.Driver.Name)
            .Select(g => new DriverProductivityDto
            {
                DriverName = g.Key,
                TripCount = g.Sum(s => s.TripCount),
                TotalFuelCost = g.Sum(s => s.FuelCost),
                TotalSettlement = g.Sum(s => s.TripIncome + s.CompanyPickupIncentive),
                NetPayout = g.Sum(s => s.NetSettlement)
            })
            .OrderByDescending(d => d.TripCount)
            .ToList();

        return Result<IReadOnlyList<DriverProductivityDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<SalesmanProductivityDto>>> GetSalesmanProductivityAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var settlements = await _context.SalesmanSettlements
            .Where(s => !s.IsDeleted && s.SettlementDate >= from && s.SettlementDate <= to)
            .Include(s => s.Salesman)
            .ToListAsync(ct);

        var report = settlements
            .GroupBy(s => s.Salesman.Name)
            .Select(g => new SalesmanProductivityDto
            {
                SalesmanName = g.Key,
                OrderCount = g.Count(),
                TotalSales = g.Sum(s => s.TotalSales),
                TotalCollection = g.Sum(s => s.Collection),
                TotalCommission = g.Sum(s => s.Commission)
            })
            .OrderByDescending(s => s.TotalSales)
            .ToList();

        return Result<IReadOnlyList<SalesmanProductivityDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<CylinderSizeSalesDto>>> GetCylinderSizeSalesAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var items = await _context.SalesOrderItems
            .Where(i => !i.IsDeleted)
            .Include(i => i.SalesOrder)
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.CylinderSize)
            .Where(i => i.SalesOrder!.OrderDate >= from && i.SalesOrder.OrderDate <= to && !i.SalesOrder.IsDeleted)
            .ToListAsync(ct);

        var report = items
            .GroupBy(i => new { Brand = i.Product.Brand?.Name ?? "Unknown", Size = i.Product.CylinderSize?.Name ?? "N/A", Weight = i.Product.CylinderSize?.WeightKg ?? 0 })
            .Select(g => new CylinderSizeSalesDto
            {
                BrandName = g.Key.Brand,
                CylinderSizeName = g.Key.Size,
                WeightKg = g.Key.Weight,
                QuantitySold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToList();

        return Result<IReadOnlyList<CylinderSizeSalesDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<AdvanceRefillReportDto>>> GetAdvanceRefillReportAsync(CancellationToken ct = default)
    {
        var orders = await _context.SalesOrders
            .Where(so => !so.IsDeleted && so.Notes != null && so.Notes.Contains("Advance Refill"))
            .Include(so => so.Customer)
            .Include(so => so.Items).ThenInclude(i => i.Product)
            .OrderByDescending(so => so.OrderDate)
            .ToListAsync(ct);

        var report = orders.SelectMany(so => so.Items.Select(i => new AdvanceRefillReportDto
        {
            CustomerName = so.Customer.Name,
            ProductName = i.Product.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalAmount = i.TotalPrice,
            OrderDate = so.OrderDate,
            OrderNumber = so.OrderNumber
        })).ToList();

        return Result<IReadOnlyList<AdvanceRefillReportDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<RefillHistoryDto>>> GetRefillHistoryAsync(Guid customerId, CancellationToken ct = default)
    {
        var items = await _context.SalesOrderItems
            .Where(i => !i.IsDeleted)
            .Include(i => i.SalesOrder).ThenInclude(so => so!.Customer)
            .Include(i => i.Product)
            .Where(i => i.SalesOrder!.CustomerId == customerId && !i.SalesOrder.IsDeleted)
            .OrderByDescending(i => i.SalesOrder!.OrderDate)
            .ToListAsync(ct);

        var report = items.Select(i => new RefillHistoryDto
        {
            CustomerName = i.SalesOrder!.Customer.Name,
            ProductName = i.Product.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalAmount = i.TotalPrice,
            OrderDate = i.SalesOrder.OrderDate,
            OrderNumber = i.SalesOrder.OrderNumber,
            Status = i.SalesOrder.Status.ToString()
        }).ToList();

        return Result<IReadOnlyList<RefillHistoryDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<VehicleSalesDto>>> GetSalesByVehicleAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var closings = await _context.VehicleClosings
            .Where(vc => !vc.IsDeleted && vc.ClosingDate >= from && vc.ClosingDate <= to)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Truck)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Driver)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Salesman)
            .ToListAsync(ct);

        var report = closings
            .GroupBy(vc => vc.VehicleLoading.Truck.Name)
            .Select(g => new VehicleSalesDto
            {
                TruckName = g.Key,
                DriverName = g.First().VehicleLoading.Driver.Name,
                SalesmanName = g.First().VehicleLoading.Salesman.Name,
                OrderCount = g.Count(),
                TotalSales = g.Sum(vc => vc.CashCollected + vc.CreditSales),
                TotalPayments = g.Sum(vc => vc.CashCollected)
            })
            .OrderByDescending(v => v.TotalSales)
            .ToList();

        return Result<IReadOnlyList<VehicleSalesDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<CreditPurchaseDto>>> GetCreditPurchasesAsync(CancellationToken ct = default)
    {
        var orders = await _context.PurchaseOrders
            .Where(po => !po.IsDeleted)
            .Include(po => po.Supplier)
            .Include(po => po.Payments)
            .ToListAsync(ct);

        var report = orders
            .Where(po => po.TotalAmount > po.Payments.Where(p => !p.IsDeleted).Sum(p => p.Amount))
            .Select(po => new CreditPurchaseDto
            {
                OrderNumber = po.OrderNumber,
                SupplierName = po.Supplier.Name,
                OrderDate = po.OrderDate,
                TotalAmount = po.TotalAmount,
                AmountPaid = po.Payments.Where(p => !p.IsDeleted).Sum(p => p.Amount),
                Outstanding = po.TotalAmount - po.Payments.Where(p => !p.IsDeleted).Sum(p => p.Amount)
            })
            .OrderByDescending(p => p.Outstanding)
            .ToList();

        return Result<IReadOnlyList<CreditPurchaseDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<CashFlowEntryDto>>> GetCashFlowAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var payments = await _context.Payments
            .Where(p => !p.IsDeleted && p.PaymentDate >= from && p.PaymentDate <= to)
            .ToListAsync(ct);

        var report = payments
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new CashFlowEntryDto
            {
                Date = g.Key,
                CashIn = g.Where(p => p.Direction == PaymentDirection.Inbound).Sum(p => p.Amount),
                CashOut = g.Where(p => p.Direction == PaymentDirection.Outbound).Sum(p => p.Amount),
                NetCashFlow = g.Where(p => p.Direction == PaymentDirection.Inbound).Sum(p => p.Amount)
                             - g.Where(p => p.Direction == PaymentDirection.Outbound).Sum(p => p.Amount)
            })
            .OrderBy(e => e.Date)
            .ToList();

        return Result<IReadOnlyList<CashFlowEntryDto>>.Success(report);
    }

    public async Task<Result<IReadOnlyList<PnLCategoryDto>>> GetPnLBreakdownAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var categories = new List<PnLCategoryDto>();

        var sales = await _context.SalesOrders.Where(so => !so.IsDeleted && so.OrderDate >= from && so.OrderDate <= to).SumAsync(so => so.TotalAmount - so.Discount, ct);
        categories.Add(new PnLCategoryDto { Category = "Sales Revenue", Amount = sales, IsIncome = true });

        var commission = await _context.PurchaseOrders.Where(po => !po.IsDeleted && po.OrderDate >= from && po.OrderDate <= to).SumAsync(po => po.CommissionEarned, ct);
        if (commission > 0) categories.Add(new PnLCategoryDto { Category = "Commission Earned", Amount = commission, IsIncome = true });

        var settlements = await _context.DriverSettlements.Where(s => !s.IsDeleted && s.SettlementDate >= from && s.SettlementDate <= to).ToListAsync(ct);
        var fuel = settlements.Sum(s => s.FuelCost);
        if (fuel > 0) categories.Add(new PnLCategoryDto { Category = "Fuel Costs", Amount = fuel, IsIncome = false });
        var loading = settlements.Sum(s => s.LoadingCost + s.UnloadingCost);
        if (loading > 0) categories.Add(new PnLCategoryDto { Category = "Loading/Unloading Costs", Amount = loading, IsIncome = false });
        var allowance = settlements.Sum(s => s.Allowance);
        if (allowance > 0) categories.Add(new PnLCategoryDto { Category = "Driver Allowance", Amount = allowance, IsIncome = false });

        var salesmanSettlements = await _context.SalesmanSettlements.Where(s => !s.IsDeleted && s.SettlementDate >= from && s.SettlementDate <= to).ToListAsync(ct);
        var commissionPaid = salesmanSettlements.Sum(s => s.Commission);
        if (commissionPaid > 0) categories.Add(new PnLCategoryDto { Category = "Salesman Commission", Amount = commissionPaid, IsIncome = false });
        var bonus = salesmanSettlements.Sum(s => s.Bonus);
        if (bonus > 0) categories.Add(new PnLCategoryDto { Category = "Salesman Bonus", Amount = bonus, IsIncome = false });
        var dailyAllowance = salesmanSettlements.Sum(s => s.DailyAllowance);
        if (dailyAllowance > 0) categories.Add(new PnLCategoryDto { Category = "Salesman Daily Allowance", Amount = dailyAllowance, IsIncome = false });

        return Result<IReadOnlyList<PnLCategoryDto>>.Success(categories);
    }
}
