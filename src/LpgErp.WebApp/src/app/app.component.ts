import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="app-container">
      <nav class="sidebar">
        <div class="brand">LPG ERP</div>
        <ul>
          <li><a routerLink="/dashboard" routerLinkActive="active">Dashboard</a></li>
          <li><a routerLink="/brands" routerLinkActive="active">Brands</a></li>
          <li><a routerLink="/warehouses" routerLinkActive="active">Warehouses</a></li>
          <li><a routerLink="/customers" routerLinkActive="active">Customers</a></li>
          <li><a routerLink="/products" routerLinkActive="active">Products</a></li>
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
    li a {
      display: block;
      padding: 0.75rem 1rem;
      color: #ccc;
      text-decoration: none;
      border-radius: 4px;
    }
    li a:hover, li a.active {
      background: #16213e;
      color: white;
    }
    .content {
      flex: 1;
      padding: 2rem;
      background: #f5f5f5;
    }
  `],
})
export class AppComponent {}
