import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h1>Dashboard</h1>
    <div class="dashboard-grid">
      <div class="card">
        <h3>Brands</h3>
        <p class="stat">{{ brandsCount() }}</p>
      </div>
      <div class="card">
        <h3>Warehouses</h3>
        <p class="stat">{{ warehousesCount() }}</p>
      </div>
      <div class="card">
        <h3>Customers</h3>
        <p class="stat">{{ customersCount() }}</p>
      </div>
      <div class="card">
        <h3>Products</h3>
        <p class="stat">{{ productsCount() }}</p>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1.5rem;
    }
    .card {
      background: white;
      border-radius: 8px;
      padding: 1.5rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .stat {
      font-size: 2rem;
      font-weight: bold;
      color: #1a1a2e;
    }
  `],
})
export class DashboardComponent implements OnInit {
  brandsCount = signal(0);
  warehousesCount = signal(0);
  customersCount = signal(0);
  productsCount = signal(0);

  ngOnInit() {
    // TODO: Load from API
  }
}
