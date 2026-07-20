import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-container">
      <nav class="sidebar">
        <div class="brand">LPG ERP</div>
        <ul>
          <li><a routerLink="/dashboard" routerLinkActive="active">Dashboard</a></li>
          <li class="section-label">Master Data</li>
          <li><a routerLink="/brands" routerLinkActive="active">Brands</a></li>
          <li><a routerLink="/warehouses" routerLinkActive="active">Warehouses</a></li>
          <li><a routerLink="/customers" routerLinkActive="active">Customers</a></li>
          <li><a routerLink="/products" routerLinkActive="active">Products</a></li>
          <li><a routerLink="/suppliers" routerLinkActive="active">Suppliers</a></li>
          <li><a routerLink="/cylinders" routerLinkActive="active">Cylinders</a></li>
          <li><a routerLink="/cylinder-sizes" routerLinkActive="active">Cylinder Sizes</a></li>
          <li><a routerLink="/routes" routerLinkActive="active">Routes</a></li>
          <li><a routerLink="/transport-companies" routerLinkActive="active">Transport Companies</a></li>
          <li class="section-label">Operations</li>
          <li><a routerLink="/trucks" routerLinkActive="active">Trucks</a></li>
          <li><a routerLink="/drivers" routerLinkActive="active">Drivers</a></li>
          <li><a routerLink="/salesmen" routerLinkActive="active">Salesmen</a></li>
          <li><a routerLink="/vehicle-loadings" routerLinkActive="active">Vehicle Loadings</a></li>
          <li class="section-label">Transactions</li>
          <li><a routerLink="/purchase-orders" routerLinkActive="active">Purchase Orders</a></li>
          <li><a routerLink="/sales-orders" routerLinkActive="active">Sales Orders</a></li>
          <li><a routerLink="/payments" routerLinkActive="active">Payments</a></li>
          <li><a routerLink="/stock-transfers" routerLinkActive="active">Stock Transfers</a></li>
          <li class="section-label">Settlements</li>
          <li><a routerLink="/driver-settlements" routerLinkActive="active">Driver Settlements</a></li>
          <li><a routerLink="/salesman-settlements" routerLinkActive="active">Salesman Settlements</a></li>
          <li><a routerLink="/cylinder-deposits" routerLinkActive="active">Cylinder Deposits</a></li>
          <li><a routerLink="/vehicle-closings" routerLinkActive="active">Vehicle Closings</a></li>
          <li><a routerLink="/cylinder-exchanges" routerLinkActive="active">Cylinder Exchanges</a></li>
          <li><a routerLink="/daily-sales-summaries" routerLinkActive="active">Daily Sales</a></li>
          <li class="section-label">Customer Management</li>
          <li><a routerLink="/customer-cylinder-ledger" routerLinkActive="active">Cylinder Ledger</a></li>
          <li><a routerLink="/customer-gas-ledger" routerLinkActive="active">Gas Ledger</a></li>
          <li><a routerLink="/customer-credit" routerLinkActive="active">Credit Management</a></li>
          <li><a routerLink="/advance-refills" routerLinkActive="active">Advance Refills</a></li>
          <li class="section-label">Reports & Notifications</li>
          <li><a routerLink="/customer-notifications" routerLinkActive="active">Notifications</a></li>
          <li><a routerLink="/reports" routerLinkActive="active">Reports</a></li>
        </ul>
      </nav>
      <main class="content">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    .app-container {
      display: flex;
      min-height: 100vh;
    }
    .sidebar {
      width: 250px;
      background: #1a1a2e;
      color: white;
      padding: 1rem;
      overflow-y: auto;
    }
    .brand {
      font-size: 1.5rem;
      font-weight: bold;
      padding: 1rem 0;
      border-bottom: 1px solid #333;
      margin-bottom: 1rem;
    }
    ul {
      list-style: none;
      padding: 0;
    }
    .section-label {
      font-size: 0.7rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: #666;
      padding: 0.75rem 1rem 0.25rem;
      font-weight: 600;
    }
    li a {
      display: block;
      padding: 0.5rem 1rem;
      color: #ccc;
      text-decoration: none;
      border-radius: 4px;
      font-size: 0.9rem;
    }
    li a:hover, li a.active {
      background: #16213e;
      color: white;
    }
    .content {
      flex: 1;
      padding: 2rem;
      background: #f5f5f5;
      overflow-y: auto;
    }
  `],
})
export class AppComponent {}
