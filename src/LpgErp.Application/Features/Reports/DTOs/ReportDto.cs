namespace LpgErp.Application.Features.Reports.DTOs;

public class InventoryReportDto
{
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
}

public class BrandInventoryDto
{
    public string BrandName { get; set; } = string.Empty;
    public int EmptyCylinderCount { get; set; }
    public int FilledCylinderCount { get; set; }
    public int TotalQuantity { get; set; }
}

public class LowStockAlertDto
{
    public string ProductName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int Deficit { get; set; }
}

public class CylinderMovementDto
{
    public DateTime Date { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? FromWarehouse { get; set; }
    public string? ToWarehouse { get; set; }
    public string? Reference { get; set; }
}

public class SalesReportDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CustomerSalesSummaryDto
{
    public string CustomerName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal Outstanding { get; set; }
}

public class ProductTypeSalesDto
{
    public string ProductType { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class PurchaseReportDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime? OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CommissionEarned { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CustomerReportDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int CylinderBalance { get; set; }
}

public class VehicleLoadingReportDto
{
    public DateTime Date { get; set; }
    public string TruckName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string SalesmanName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class FinancialReportDto
{
    public decimal TotalSales { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalPurchasePayments { get; set; }
    public decimal AccountsReceivable { get; set; }
    public decimal SupplierPayable { get; set; }
    public decimal TransportationExpenses { get; set; }
    public decimal CommissionBalance { get; set; }
    public decimal DepositLiability { get; set; }
    public decimal NetProfit => TotalSales - TotalPurchases - TransportationExpenses;
}

public class RouteSalesDto
{
    public string? RouteName { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal Outstanding { get; set; }
}

public class BrandSalesDto
{
    public string BrandName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class VehicleReconciliationDto
{
    public DateTime Date { get; set; }
    public string TruckName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string SalesmanName { get; set; } = string.Empty;
    public int TotalLoaded { get; set; }
    public int TotalSold { get; set; }
    public int TotalReturned { get; set; }
    public int TotalDamaged { get; set; }
    public int Variance { get; set; }
    public decimal CashCollected { get; set; }
    public decimal CreditSales { get; set; }
}

public class DriverProductivityDto
{
    public string DriverName { get; set; } = string.Empty;
    public int TripCount { get; set; }
    public decimal TotalFuelCost { get; set; }
    public decimal TotalSettlement { get; set; }
    public decimal NetPayout { get; set; }
}

public class SalesmanProductivityDto
{
    public string SalesmanName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalCollection { get; set; }
    public decimal TotalCommission { get; set; }
}

public class CylinderSizeSalesDto
{
    public string BrandName { get; set; } = string.Empty;
    public string CylinderSizeName { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class AdvanceRefillReportDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
}

public class RefillHistoryDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class VehicleSalesDto
{
    public string TruckName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string SalesmanName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalPayments { get; set; }
}

public class CreditPurchaseDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime? OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Outstanding { get; set; }
}

public class CashFlowEntryDto
{
    public DateTime Date { get; set; }
    public decimal CashIn { get; set; }
    public decimal CashOut { get; set; }
    public decimal NetCashFlow { get; set; }
}

public class PnLCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsIncome { get; set; }
}
