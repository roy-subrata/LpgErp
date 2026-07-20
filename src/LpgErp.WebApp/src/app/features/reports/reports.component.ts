import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { InventoryReport, FinancialReport, BrandInventory, LowStockAlert, CylinderMovement, SalesReport, CustomerSalesSummary, ProductTypeSales, PurchaseReport, CustomerReport, VehicleLoadingReport, RouteSales, BrandSales, VehicleReconciliation, DriverProductivity, SalesmanProductivity, CylinderSizeSales, AdvanceRefillReport, VehicleSales, CreditPurchase, CashFlowEntry, PnLCategory } from '../../core/models';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <h1>Reports</h1>
      <div class="date-range">
        <label>From</label><input type="date" [(ngModel)]="fromDate" />
        <label>To</label><input type="date" [(ngModel)]="toDate" />
      </div>
    </div>

    <div class="report-grid">
      <div class="report-card" (click)="load('inventory')"><h3>Inventory</h3><p>Stock levels by warehouse</p></div>
      <div class="report-card" (click)="load('brandInventory')"><h3>Brand Inventory</h3><p>Stock by LPG brand</p></div>
      <div class="report-card" (click)="load('lowStock')"><h3>Low Stock Alerts</h3><p>Products below minimum</p></div>
      <div class="report-card" (click)="load('movements')"><h3>Cylinder Movements</h3><p>Stock movement history</p></div>
      <div class="report-card" (click)="load('sales')"><h3>Sales Report</h3><p>All sales orders</p></div>
      <div class="report-card" (click)="load('salesByCustomer')"><h3>Sales by Customer</h3><p>Customer-wise summary</p></div>
      <div class="report-card" (click)="load('salesByBrand')"><h3>Sales by Brand</h3><p>Brand-wise sales</p></div>
      <div class="report-card" (click)="load('salesByRoute')"><h3>Sales by Route</h3><p>Route-wise sales</p></div>
      <div class="report-card" (click)="load('productTypeSales')"><h3>Sales by Product Type</h3><p>Package vs Refill vs Accessory</p></div>
      <div class="report-card" (click)="load('cylinderSizeSales')"><h3>Sales by Cylinder Size</h3><p>Brand and size-wise sales</p></div>
      <div class="report-card" (click)="load('vehicleSales')"><h3>Sales by Vehicle</h3><p>Revenue per truck</p></div>
      <div class="report-card" (click)="load('purchases')"><h3>Purchase Report</h3><p>All purchase orders</p></div>
      <div class="report-card" (click)="load('creditPurchases')"><h3>Credit Purchases</h3><p>Outstanding supplier balances</p></div>
      <div class="report-card" (click)="load('advanceRefillReport')"><h3>Advance Refill Report</h3><p>Advance refill orders</p></div>
      <div class="report-card" (click)="load('customers')"><h3>Customer Report</h3><p>Outstanding balances</p></div>
      <div class="report-card" (click)="load('vehicles')"><h3>Vehicle Loading</h3><p>Loading/dispatch history</p></div>
      <div class="report-card" (click)="load('vehicleReconciliation')"><h3>Vehicle Reconciliation</h3><p>Loaded vs Sold vs Returned</p></div>
      <div class="report-card" (click)="load('driverProductivity')"><h3>Driver Productivity</h3><p>Trips and costs</p></div>
      <div class="report-card" (click)="load('salesmanProductivity')"><h3>Salesman Productivity</h3><p>Sales and commissions</p></div>
      <div class="report-card highlight-card" (click)="load('financial')"><h3>Financial Report</h3><p>Sales, purchases, profit</p></div>
      <div class="report-card" (click)="load('cashflow')"><h3>Cash Flow</h3><p>Daily cash in/out</p></div>
      <div class="report-card" (click)="load('pnl')"><h3>P&L Breakdown</h3><p>Income vs expenses by category</p></div>
    </div>

    @if (activeReport() && activeReport() !== 'financial') {
      <div class="report-section">
        <h2>{{ reportTitle() }}</h2>
        <div class="table-container">
          <table>
            <thead>
              <tr>
                @for (col of tableColumns(); track col) {
                  <th>{{ col }}</th>
                }
              </tr>
            </thead>
            <tbody>
              @for (row of tableData(); track $index) {
                <tr>
                  @for (col of tableKeys(); track col; let i = $index) {
                    <td>{{ formatCell(row, col) }}</td>
                  }
                </tr>
              } @empty {
                <tr><td [attr.colspan]="tableColumns().length">No data found.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }

    @if (financialReport()) {
      <div class="report-section">
        <h2>Financial Report</h2>
        <div class="financial-grid">
          <div class="metric"><label>Total Sales</label><span>{{ financialReport()!.totalSales | number:'1.2-2' }}</span></div>
          <div class="metric"><label>Total Payments</label><span>{{ financialReport()!.totalPayments | number:'1.2-2' }}</span></div>
          <div class="metric"><label>Total Purchases</label><span>{{ financialReport()!.totalPurchases | number:'1.2-2' }}</span></div>
          <div class="metric"><label>Transport Expenses</label><span>{{ financialReport()!.transportationExpenses | number:'1.2-2' }}</span></div>
          <div class="metric"><label>Commission Earned</label><span>{{ financialReport()!.commissionBalance | number:'1.2-2' }}</span></div>
          <div class="metric"><label>Accounts Receivable</label><span>{{ financialReport()!.accountsReceivable | number:'1.2-2' }}</span></div>
          <div class="metric"><label>Supplier Payable</label><span>{{ financialReport()!.supplierPayable | number:'1.2-2' }}</span></div>
          <div class="metric"><label>Deposit Liability</label><span>{{ financialReport()!.depositLiability | number:'1.2-2' }}</span></div>
          <div class="metric highlight"><label>Net Profit</label><span>{{ financialReport()!.netProfit | number:'1.2-2' }}</span></div>
        </div>
      </div>
    }

    @if (pnlData().length > 0) {
      <div class="report-section">
        <h2>Profit & Loss Breakdown</h2>
        <div class="table-container">
          <table>
            <thead><tr><th>Category</th><th>Amount</th><th>Type</th></tr></thead>
            <tbody>
              @for (item of pnlData(); track $index) {
                <tr>
                  <td>{{ item.category }}</td>
                  <td>{{ item.amount | number:'1.2-2' }}</td>
                  <td [style.color]="item.isIncome ? '#28a745' : '#dc3545'">{{ item.isIncome ? 'Income' : 'Expense' }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .date-range { display: flex; gap: 0.5rem; align-items: center; }
    .date-range label { font-size: 0.85rem; color: #666; }
    .date-range input { padding: 0.4rem; border: 1px solid #ddd; border-radius: 4px; }
    .report-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin-bottom: 2rem; }
    .report-card { background: white; border-radius: 8px; padding: 1.25rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1); cursor: pointer; transition: box-shadow 0.2s; }
    .report-card:hover { box-shadow: 0 4px 8px rgba(0,0,0,0.15); }
    .report-card h3 { margin: 0 0 0.25rem; font-size: 0.95rem; }
    .report-card p { margin: 0; color: #666; font-size: 0.8rem; }
    .highlight-card { border-left: 3px solid #1a1a2e; }
    .report-section { margin-bottom: 2rem; }
    .report-section h2 { margin-bottom: 1rem; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.6rem 0.8rem; text-align: left; border-bottom: 1px solid #eee; white-space: nowrap; }
    th { background: #f8f9fa; font-weight: 600; font-size: 0.85rem; }
    td { font-size: 0.85rem; }
    .financial-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; }
    .metric { background: white; border-radius: 8px; padding: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    .metric label { display: block; font-size: 0.85rem; color: #666; margin-bottom: 0.25rem; }
    .metric span { font-size: 1.25rem; font-weight: 600; }
    .metric.highlight span { color: #28a745; }
  `],
})
export class ReportsComponent {
  private api = inject(ApiService);
  fromDate = '';
  toDate = '';
  activeReport = signal<string | null>(null);
  reportTitle = signal('');
  tableColumns = signal<string[]>([]);
  tableKeys = signal<string[]>([]);
  tableData = signal<any[]>([]);
  financialReport = signal<FinancialReport | null>(null);
  pnlData = signal<PnLCategory[]>([]);

  private getDateRange() {
    const from = this.fromDate ? new Date(this.fromDate) : new Date(new Date().setMonth(new Date().getMonth() - 1));
    const to = this.toDate ? new Date(this.toDate) : new Date();
    return { from: from.toISOString(), to: to.toISOString() };
  }

  load(report: string) {
    this.financialReport.set(null);
    this.pnlData.set([]);
    const { from, to } = this.getDateRange();
    const params = { from, to };

    switch (report) {
      case 'inventory':
        this.activeReport.set('inventory');
        this.reportTitle.set('Inventory Report');
        this.tableColumns.set(['Product', 'Brand', 'Warehouse', 'Quantity', 'Type']);
        this.tableKeys.set(['productName', 'brandName', 'warehouseName', 'quantity', 'productType']);
        this.api.getAll<InventoryReport>('reports/inventory').subscribe(d => this.tableData.set(d.items));
        break;
      case 'brandInventory':
        this.activeReport.set('brandInventory');
        this.reportTitle.set('Brand Inventory');
        this.tableColumns.set(['Brand', 'Empty Cylinders', 'Filled Cylinders', 'Total']);
        this.tableKeys.set(['brandName', 'emptyCylinderCount', 'filledCylinderCount', 'totalQuantity']);
        this.api.getAll<BrandInventory>('reports/inventory/brands').subscribe(d => this.tableData.set(d.items));
        break;
      case 'lowStock':
        this.activeReport.set('lowStock');
        this.reportTitle.set('Low Stock Alerts');
        this.tableColumns.set(['Product', 'Brand', 'Warehouse', 'Current Stock', 'Minimum', 'Deficit']);
        this.tableKeys.set(['productName', 'brandName', 'warehouseName', 'currentStock', 'minimumStock', 'deficit']);
        this.api.getAll<LowStockAlert>('reports/inventory/low-stock').subscribe(d => this.tableData.set(d.items));
        break;
      case 'movements':
        this.activeReport.set('movements');
        this.reportTitle.set('Cylinder Movement History');
        this.tableColumns.set(['Date', 'Product', 'Type', 'Quantity', 'From', 'To', 'Reference']);
        this.tableKeys.set(['date', 'productName', 'type', 'quantity', 'fromWarehouse', 'toWarehouse', 'reference']);
        this.api.getAll<CylinderMovement>('reports/inventory/movements', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'sales':
        this.activeReport.set('sales');
        this.reportTitle.set('Sales Report');
        this.tableColumns.set(['Order #', 'Customer', 'Date', 'Total', 'Discount', 'Net', 'Status']);
        this.tableKeys.set(['orderNumber', 'customerName', 'orderDate', 'totalAmount', 'discount', 'netAmount', 'status']);
        this.api.getAll<SalesReport>('reports/sales', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'salesByCustomer':
        this.activeReport.set('salesByCustomer');
        this.reportTitle.set('Sales by Customer');
        this.tableColumns.set(['Customer', 'Orders', 'Total Purchases', 'Payments', 'Outstanding']);
        this.tableKeys.set(['customerName', 'orderCount', 'totalPurchases', 'totalPayments', 'outstanding']);
        this.api.getAll<CustomerSalesSummary>('reports/sales/by-customer', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'salesByBrand':
        this.activeReport.set('salesByBrand');
        this.reportTitle.set('Sales by Brand');
        this.tableColumns.set(['Brand', 'Quantity Sold', 'Total Revenue']);
        this.tableKeys.set(['brandName', 'quantitySold', 'totalRevenue']);
        this.api.getAll<BrandSales>('reports/sales/by-brand', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'salesByRoute':
        this.activeReport.set('salesByRoute');
        this.reportTitle.set('Sales by Route');
        this.tableColumns.set(['Route', 'Orders', 'Total Sales', 'Payments', 'Outstanding']);
        this.tableKeys.set(['routeName', 'orderCount', 'totalSales', 'totalPayments', 'outstanding']);
        this.api.getAll<RouteSales>('reports/sales/by-route', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'productTypeSales':
        this.activeReport.set('productTypeSales');
        this.reportTitle.set('Sales by Product Type');
        this.tableColumns.set(['Product Type', 'Quantity Sold', 'Total Revenue']);
        this.tableKeys.set(['productType', 'quantitySold', 'totalRevenue']);
        this.api.getAll<ProductTypeSales>('reports/sales/by-product-type', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'purchases':
        this.activeReport.set('purchases');
        this.reportTitle.set('Purchase Report');
        this.tableColumns.set(['Order #', 'Supplier', 'Date', 'Total', 'Commission', 'Status']);
        this.tableKeys.set(['orderNumber', 'supplierName', 'orderDate', 'totalAmount', 'commissionEarned', 'status']);
        this.api.getAll<PurchaseReport>('reports/purchases', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'customers':
        this.activeReport.set('customers');
        this.reportTitle.set('Customer Report');
        this.tableColumns.set(['Customer', 'Code', 'Credit Limit', 'Total Purchases', 'Payments', 'Outstanding', 'Cylinder Balance']);
        this.tableKeys.set(['customerName', 'customerCode', 'creditLimit', 'totalPurchases', 'totalPayments', 'outstandingBalance', 'cylinderBalance']);
        this.api.getAll<CustomerReport>('reports/customers').subscribe(d => this.tableData.set(d.items));
        break;
      case 'vehicles':
        this.activeReport.set('vehicles');
        this.reportTitle.set('Vehicle Loading Report');
        this.tableColumns.set(['Date', 'Truck', 'Driver', 'Salesman', 'Warehouse', 'Status', 'Items']);
        this.tableKeys.set(['date', 'truckName', 'driverName', 'salesmanName', 'warehouseName', 'status', 'itemCount']);
        this.api.getAll<VehicleLoadingReport>('reports/vehicles', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'vehicleReconciliation':
        this.activeReport.set('vehicleReconciliation');
        this.reportTitle.set('Vehicle Reconciliation');
        this.tableColumns.set(['Date', 'Truck', 'Driver', 'Salesman', 'Loaded', 'Sold', 'Returned', 'Damaged', 'Variance', 'Cash', 'Credit']);
        this.tableKeys.set(['date', 'truckName', 'driverName', 'salesmanName', 'totalLoaded', 'totalSold', 'totalReturned', 'totalDamaged', 'variance', 'cashCollected', 'creditSales']);
        this.api.getAll<VehicleReconciliation>('reports/vehicles/reconciliation', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'driverProductivity':
        this.activeReport.set('driverProductivity');
        this.reportTitle.set('Driver Productivity');
        this.tableColumns.set(['Driver', 'Trips', 'Fuel Cost', 'Total Settlement', 'Net Payout']);
        this.tableKeys.set(['driverName', 'tripCount', 'totalFuelCost', 'totalSettlement', 'netPayout']);
        this.api.getAll<DriverProductivity>('reports/drivers/productivity', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'salesmanProductivity':
        this.activeReport.set('salesmanProductivity');
        this.reportTitle.set('Salesman Productivity');
        this.tableColumns.set(['Salesman', 'Orders', 'Total Sales', 'Collection', 'Commission']);
        this.tableKeys.set(['salesmanName', 'orderCount', 'totalSales', 'totalCollection', 'totalCommission']);
        this.api.getAll<SalesmanProductivity>('reports/salesmen/productivity', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'financial':
        this.activeReport.set(null);
        this.api.get<FinancialReport>('reports/financial', params).subscribe(data => this.financialReport.set(data));
        this.api.getAll<PnLCategory>('reports/financial/pnl', 1, 100, params).subscribe(d => this.pnlData.set(d.items));
        break;
      case 'cylinderSizeSales':
        this.activeReport.set('cylinderSizeSales');
        this.reportTitle.set('Sales by Cylinder Size');
        this.tableColumns.set(['Brand', 'Cylinder Size', 'Weight (kg)', 'Quantity Sold', 'Total Revenue']);
        this.tableKeys.set(['brandName', 'cylinderSizeName', 'weightKg', 'quantitySold', 'totalRevenue']);
        this.api.getAll<CylinderSizeSales>('reports/sales/by-cylinder-size', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'vehicleSales':
        this.activeReport.set('vehicleSales');
        this.reportTitle.set('Sales by Vehicle');
        this.tableColumns.set(['Truck', 'Driver', 'Salesman', 'Trips', 'Total Sales', 'Cash Collected']);
        this.tableKeys.set(['truckName', 'driverName', 'salesmanName', 'orderCount', 'totalSales', 'totalPayments']);
        this.api.getAll<VehicleSales>('reports/vehicles/sales', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'creditPurchases':
        this.activeReport.set('creditPurchases');
        this.reportTitle.set('Credit Purchases');
        this.tableColumns.set(['Order #', 'Supplier', 'Date', 'Total Amount', 'Paid', 'Outstanding']);
        this.tableKeys.set(['orderNumber', 'supplierName', 'orderDate', 'totalAmount', 'amountPaid', 'outstanding']);
        this.api.getAll<CreditPurchase>('reports/purchases/credit').subscribe(d => this.tableData.set(d.items));
        break;
      case 'advanceRefillReport':
        this.activeReport.set('advanceRefillReport');
        this.reportTitle.set('Advance Refill Report');
        this.tableColumns.set(['Customer', 'Product', 'Quantity', 'Unit Price', 'Total', 'Date', 'Order #']);
        this.tableKeys.set(['customerName', 'productName', 'quantity', 'unitPrice', 'totalAmount', 'orderDate', 'orderNumber']);
        this.api.getAll<AdvanceRefillReport>('reports/advance-refills').subscribe(d => this.tableData.set(d.items));
        break;
      case 'cashflow':
        this.activeReport.set('cashflow');
        this.reportTitle.set('Cash Flow');
        this.tableColumns.set(['Date', 'Cash In', 'Cash Out', 'Net Cash Flow']);
        this.tableKeys.set(['date', 'cashIn', 'cashOut', 'netCashFlow']);
        this.api.getAll<CashFlowEntry>('reports/financial/cashflow', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
      case 'pnl':
        this.activeReport.set('pnl');
        this.reportTitle.set('P&L Breakdown');
        this.tableColumns.set(['Category', 'Amount', 'Type']);
        this.tableKeys.set(['category', 'amount', 'isIncome']);
        this.api.getAll<PnLCategory>('reports/financial/pnl', 1, 100, params).subscribe(d => this.tableData.set(d.items));
        break;
    }
  }

  formatCell(row: any, key: string): string {
    const val = row[key];
    if (val == null) return '';
    if (key === 'isIncome') return val ? 'Income' : 'Expense';
    if (key.toLowerCase().includes('date')) return new Date(val).toLocaleDateString();
    if (typeof val === 'number' && (key.toLowerCase().includes('amount') || key.toLowerCase().includes('price') || key.toLowerCase().includes('total') || key.toLowerCase().includes('sales') || key.toLowerCase().includes('cost') || key.toLowerCase().includes('revenue') || key.toLowerCase().includes('payout') || key.toLowerCase().includes('collection') || key.toLowerCase().includes('commission') || key.toLowerCase().includes('outstanding') || key.toLowerCase().includes('credit') || key.toLowerCase().includes('cash') || key.toLowerCase().includes('liability') || key.toLowerCase().includes('payable') || key.toLowerCase().includes('receivable') || key.toLowerCase().includes('purchases') || key.toLowerCase().includes('payments'))) {
      return val.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
    return String(val);
  }
}
