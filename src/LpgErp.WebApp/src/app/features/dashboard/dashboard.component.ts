import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { FinancialReport, LowStockAlert } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <h1>Dashboard</h1>

    <div class="section-title">Master Data</div>
    <div class="dashboard-grid">
      <a class="card" routerLink="/brands">
        <h3>Brands</h3>
        <p class="stat">{{ brandsCount() }}</p>
      </a>
      <a class="card" routerLink="/warehouses">
        <h3>Warehouses</h3>
        <p class="stat">{{ warehousesCount() }}</p>
      </a>
      <a class="card" routerLink="/customers">
        <h3>Customers</h3>
        <p class="stat">{{ customersCount() }}</p>
      </a>
      <a class="card" routerLink="/products">
        <h3>Products</h3>
        <p class="stat">{{ productsCount() }}</p>
      </a>
      <a class="card" routerLink="/suppliers">
        <h3>Suppliers</h3>
        <p class="stat">{{ suppliersCount() }}</p>
      </a>
      <a class="card" routerLink="/trucks">
        <h3>Trucks</h3>
        <p class="stat">{{ trucksCount() }}</p>
      </a>
      <a class="card" routerLink="/drivers">
        <h3>Drivers</h3>
        <p class="stat">{{ driversCount() }}</p>
      </a>
      <a class="card" routerLink="/salesmen">
        <h3>Salesmen</h3>
        <p class="stat">{{ salesmenCount() }}</p>
      </a>
    </div>

    <div class="section-title">Operations</div>
    <div class="dashboard-grid">
      <a class="card" routerLink="/sales-orders">
        <h3>Sales Orders</h3>
        <p class="stat">{{ salesOrdersCount() }}</p>
      </a>
      <a class="card" routerLink="/purchase-orders">
        <h3>Purchase Orders</h3>
        <p class="stat">{{ purchaseOrdersCount() }}</p>
      </a>
      <a class="card" routerLink="/vehicle-loadings">
        <h3>Vehicle Loadings</h3>
        <p class="stat">{{ vehicleLoadingsCount() }}</p>
      </a>
      <a class="card" routerLink="/payments">
        <h3>Payments</h3>
        <p class="stat">{{ paymentsCount() }}</p>
      </a>
    </div>

    <div class="section-title">Financial Overview</div>
    <div class="dashboard-grid">
      <div class="card kpi">
        <h3>Total Sales</h3>
        <p class="stat money">{{ financial().totalSales | number:'1.2-2' }}</p>
      </div>
      <div class="card kpi">
        <h3>Total Payments</h3>
        <p class="stat money">{{ financial().totalPayments | number:'1.2-2' }}</p>
      </div>
      <div class="card kpi">
        <h3>Accounts Receivable</h3>
        <p class="stat money warning">{{ financial().accountsReceivable | number:'1.2-2' }}</p>
      </div>
      <div class="card kpi">
        <h3>Supplier Payable</h3>
        <p class="stat money warning">{{ financial().supplierPayable | number:'1.2-2' }}</p>
      </div>
      <div class="card kpi">
        <h3>Net Profit</h3>
        <p class="stat money" [class.positive]="financial().netProfit >= 0" [class.negative]="financial().netProfit < 0">
          {{ financial().netProfit | number:'1.2-2' }}
        </p>
      </div>
      <div class="card kpi">
        <h3>Deposit Liability</h3>
        <p class="stat money">{{ financial().depositLiability | number:'1.2-2' }}</p>
      </div>
    </div>

    @if (lowStockAlerts().length > 0) {
      <div class="section-title">Low Stock Alerts</div>
      <div class="table-container">
        <table>
          <thead>
            <tr>
              <th>Product</th>
              <th>Brand</th>
              <th>Warehouse</th>
              <th>Current Stock</th>
              <th>Minimum Stock</th>
              <th>Deficit</th>
            </tr>
          </thead>
          <tbody>
            @for (alert of lowStockAlerts(); track $index) {
              <tr class="alert-row">
                <td>{{ alert.productName }}</td>
                <td>{{ alert.brandName }}</td>
                <td>{{ alert.warehouseName }}</td>
                <td>{{ alert.currentStock }}</td>
                <td>{{ alert.minimumStock }}</td>
                <td class="text-danger">{{ alert.deficit }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
  styles: [`
    .section-title { font-size: 1rem; font-weight: 600; color: #555; margin: 1.5rem 0 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; }
    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 1rem;
    }
    .card {
      background: white;
      border-radius: 8px;
      padding: 1.25rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      text-decoration: none;
      color: inherit;
      transition: box-shadow 0.2s;
      cursor: pointer;
    }
    a.card:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.15); }
    .card h3 { font-size: 0.85rem; color: #888; margin: 0 0 0.5rem; font-weight: 500; }
    .stat { font-size: 1.75rem; font-weight: bold; color: #1a1a2e; margin: 0; }
    .stat.money { font-size: 1.25rem; }
    .stat.money.warning { color: #e67e22; }
    .stat.money.positive { color: #27ae60; }
    .stat.money.negative { color: #dc3545; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; margin-bottom: 1rem; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.6rem 1rem; text-align: left; border-bottom: 1px solid #eee; }
    th { background: #f8f9fa; font-weight: 600; font-size: 0.85rem; }
    .alert-row { background: #fff8e1; }
    .text-danger { color: #dc3545; font-weight: 600; }
  `],
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);

  brandsCount = signal(0);
  warehousesCount = signal(0);
  customersCount = signal(0);
  productsCount = signal(0);
  suppliersCount = signal(0);
  trucksCount = signal(0);
  driversCount = signal(0);
  salesmenCount = signal(0);
  salesOrdersCount = signal(0);
  purchaseOrdersCount = signal(0);
  vehicleLoadingsCount = signal(0);
  paymentsCount = signal(0);
  financial = signal<FinancialReport>({
    totalSales: 0, totalPayments: 0, totalPurchases: 0,
    totalPurchasePayments: 0, accountsReceivable: 0, supplierPayable: 0,
    transportationExpenses: 0, commissionBalance: 0, depositLiability: 0, netProfit: 0,
  });
  lowStockAlerts = signal<LowStockAlert[]>([]);

  ngOnInit() {
    this.loadCount('brands', this.brandsCount);
    this.loadCount('warehouses', this.warehousesCount);
    this.loadCount('customers', this.customersCount);
    this.loadCount('products', this.productsCount);
    this.loadCount('suppliers', this.suppliersCount);
    this.loadCount('trucks', this.trucksCount);
    this.loadCount('drivers', this.driversCount);
    this.loadCount('salesmen', this.salesmenCount);
    this.loadCount('salesorders', this.salesOrdersCount);
    this.loadCount('purchaseorders', this.purchaseOrdersCount);
    this.loadCount('vehicleloadings', this.vehicleLoadingsCount);
    this.loadCount('payments', this.paymentsCount);

    const today = new Date();
    const from = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0];
    const to = today.toISOString().split('T')[0];
    this.api.get<FinancialReport>('reports/financial', { from, to }).subscribe(data => this.financial.set(data));
    this.api.getAll<LowStockAlert>('reports/inventory/low-stock', 1, 100).subscribe(data => this.lowStockAlerts.set(data.items));
  }

  private loadCount(endpoint: string, target: ReturnType<typeof signal<number>>) {
    this.api.getAll<any>(endpoint, 1, 1).subscribe(r => target.set(r.pagination.totalCount));
  }
}
