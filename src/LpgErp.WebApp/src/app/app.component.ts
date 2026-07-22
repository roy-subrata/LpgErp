import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { filter, map } from 'rxjs';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

interface NavGroup {
  title: string;
  items: NavItem[];
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-shell">
      <nav class="sidebar">
        <div class="sidebar-header">
          <div class="brand-logo">L</div>
          <div class="brand-text">
            <div class="brand-title">LPG ERP</div>
            <div class="brand-subtitle">DISTRIBUTOR SUITE</div>
          </div>
        </div>

        <div class="nav-body">
          @for (group of navGroups; track group.title) {
            <div class="nav-group-label">{{ group.title }}</div>
            @for (item of group.items; track item.route) {
              <a class="nav-item" [routerLink]="item.route" routerLinkActive="active">
                <span class="nav-icon">{{ item.icon }}</span>
                <span class="nav-label">{{ item.label }}</span>
              </a>
            }
          }
        </div>

        <div class="sidebar-footer">
          <div class="user-avatar">SR</div>
          <div class="user-info">
            <div class="user-name">Subrata Roy</div>
            <div class="user-role">Admin</div>
          </div>
        </div>
      </nav>

      <div class="main-area">
        <header class="topbar">
          <div class="breadcrumb">
            <span class="breadcrumb-muted">LPG ERP</span>
            <span class="breadcrumb-sep">/</span>
            <span class="breadcrumb-active">{{ currentPageLabel() }}</span>
          </div>
          <div class="topbar-right">
            <div class="search-pill">
              <span class="search-icon">🔍</span>
              <input type="text" placeholder="Search..." class="search-input" />
              <span class="search-shortcut">⌘K</span>
            </div>
            <button class="bell-btn">
              🔔
              <span class="bell-dot"></span>
            </button>
          </div>
        </header>

        <main class="content">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; height: 100vh; overflow: hidden; }

    .app-shell {
      display: flex;
      height: 100vh;
    }

    /* ===== SIDEBAR ===== */
    .sidebar {
      width: 232px;
      min-width: 232px;
      background: var(--sidebar-bg);
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    .sidebar-header {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 18px 18px 16px;
      border-bottom: 1px solid var(--sidebar-divider);
    }

    .brand-logo {
      width: 32px;
      height: 32px;
      min-width: 32px;
      border-radius: 8px;
      background: var(--gradient);
      display: flex;
      align-items: center;
      justify-content: center;
      color: #fff;
      font-weight: 800;
      font-size: 15px;
    }

    .brand-title {
      font-size: 14px;
      font-weight: 700;
      color: #fff;
      line-height: 1.2;
    }

    .brand-subtitle {
      font-size: 10px;
      color: var(--sidebar-label);
      text-transform: uppercase;
      letter-spacing: 0.08em;
      line-height: 1.3;
    }

    .nav-body {
      flex: 1;
      overflow-y: auto;
      padding: 8px 10px;
    }

    .nav-group-label {
      font-size: 10px;
      font-weight: 700;
      color: var(--sidebar-label);
      text-transform: uppercase;
      letter-spacing: 0.12em;
      padding: 14px 10px 5px;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 7px 10px;
      border-radius: 7px;
      font-size: 13px;
      font-weight: 500;
      color: var(--sidebar-muted);
      text-decoration: none;
      transition: background 0.15s, color 0.15s;
      cursor: pointer;
    }

    .nav-item:hover {
      background: rgba(255,255,255,0.06);
      color: #fff;
      text-decoration: none;
    }

    .nav-item.active {
      background: rgba(249,115,22,0.16);
      color: #fff;
    }

    .nav-icon {
      font-size: 18px;
      opacity: 0.85;
      width: 18px;
      text-align: center;
    }

    .sidebar-footer {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 12px 18px;
      border-top: 1px solid var(--sidebar-divider);
    }

    .user-avatar {
      width: 30px;
      height: 30px;
      min-width: 30px;
      border-radius: 50%;
      background: #374151;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #e5e7eb;
      font-size: 11px;
      font-weight: 600;
    }

    .user-name {
      font-size: 12px;
      font-weight: 600;
      color: var(--sidebar-text);
      line-height: 1.2;
    }

    .user-role {
      font-size: 11px;
      color: var(--sidebar-label);
    }

    /* ===== MAIN ===== */
    .main-area {
      flex: 1;
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .topbar {
      height: 56px;
      min-height: 56px;
      background: var(--surface);
      border-bottom: 1px solid var(--border);
      padding: 0 24px;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .breadcrumb {
      font-size: 13px;
      display: flex;
      align-items: center;
      gap: 6px;
    }

    .breadcrumb-muted {
      color: var(--text-faint);
    }

    .breadcrumb-sep {
      color: var(--text-faint);
    }

    .breadcrumb-active {
      color: var(--text-primary);
      font-weight: 600;
    }

    .topbar-right {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .search-pill {
      display: flex;
      align-items: center;
      gap: 8px;
      background: var(--fill-subtle);
      border: 1px solid var(--border);
      border-radius: 8px;
      padding: 0 12px;
      height: 34px;
      width: 260px;
    }

    .search-icon {
      font-size: 14px;
      opacity: 0.5;
    }

    .search-input {
      flex: 1;
      border: none;
      background: transparent;
      outline: none;
      font-size: 13px;
      color: var(--text-primary);
    }

    .search-input::placeholder {
      color: var(--text-faint);
    }

    .search-shortcut {
      font-family: var(--font-mono);
      font-size: 11px;
      font-weight: 500;
      color: var(--text-muted);
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: 4px;
      padding: 1px 5px;
    }

    .bell-btn {
      position: relative;
      width: 34px;
      height: 34px;
      border-radius: 8px;
      border: 1px solid var(--border);
      background: var(--surface);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;
    }

    .bell-btn:hover {
      background: var(--fill-subtle);
    }

    .bell-dot {
      position: absolute;
      top: 7px;
      right: 8px;
      width: 7px;
      height: 7px;
      background: var(--red-fg);
      border-radius: 50%;
      border: 1.5px solid var(--surface);
    }

    .content {
      flex: 1;
      overflow-y: auto;
      padding: 24px 28px 40px;
    }
  `],
})
export class AppComponent {
  navGroups: NavGroup[] = [
    {
      title: 'Overview',
      items: [
        { label: 'Dashboard', icon: '◫', route: 'dashboard' },
        { label: 'Reports', icon: '▤', route: 'reports' },
      ],
    },
    {
      title: 'Operations',
      items: [
        { label: 'Vehicle Loadings', icon: '📥', route: 'vehicle-loadings' },
        { label: 'Vehicle Closings', icon: '📤', route: 'vehicle-closings' },
        { label: 'Salesmen', icon: '🧑', route: 'salesmen' },
        { label: 'Drivers', icon: '🧑‍✈️', route: 'drivers' },
        { label: 'Trucks', icon: '🚛', route: 'trucks' },
        { label: 'Routes', icon: '🗺', route: 'routes' },
      ],
    },
    {
      title: 'Transactions',
      items: [
        { label: 'Sales Orders', icon: '📋', route: 'sales-orders' },
        { label: 'Purchase Orders', icon: '📦', route: 'purchase-orders' },
        { label: 'Payments', icon: '৳', route: 'payments' },
        { label: 'Inventory', icon: '🗄', route: 'inventory' },
        { label: 'Stock Transfers', icon: '⇄', route: 'stock-transfers' },
        { label: 'Daily Sales', icon: '📊', route: 'daily-sales-summaries' },
      ],
    },
    {
      title: 'Settlements',
      items: [
        { label: 'Driver Settlements', icon: '⚖', route: 'driver-settlements' },
        { label: 'Salesman Settlements', icon: '⚖', route: 'salesman-settlements' },
        { label: 'Cylinder Deposits', icon: '◉', route: 'cylinder-deposits' },
        { label: 'Cylinder Exchanges', icon: '⇄', route: 'cylinder-exchanges' },
      ],
    },
    {
      title: 'Customers',
      items: [
        { label: 'Customers', icon: '👥', route: 'customers' },
        { label: 'Cylinder Ledger', icon: '☰', route: 'customer-cylinder-ledger' },
        { label: 'Gas Ledger', icon: '☰', route: 'customer-gas-ledger' },
        { label: 'Credit', icon: '💳', route: 'customer-credit' },
        { label: 'Advance Refills', icon: '↻', route: 'advance-refills' },
        { label: 'Notifications', icon: '🔔', route: 'customer-notifications' },
      ],
    },
    {
      title: 'Master Data',
      items: [
        { label: 'Brands', icon: '◈', route: 'brands' },
        { label: 'Warehouses', icon: '🏭', route: 'warehouses' },
        { label: 'Products', icon: '📦', route: 'products' },
        { label: 'Cylinders', icon: '🛢', route: 'cylinders' },
        { label: 'Cylinder Sizes', icon: '◎', route: 'cylinder-sizes' },
        { label: 'Suppliers', icon: '🏢', route: 'suppliers' },
        { label: 'Transport Companies', icon: '🚚', route: 'transport-companies' },
      ],
    },
  ];

  currentPageLabel = signal('Dashboard');

  private routeLabelMap: Record<string, string> = {
    'dashboard': 'Dashboard',
    'reports': 'Reports',
    'vehicle-loadings': 'Vehicle Loadings',
    'vehicle-closings': 'Vehicle Closings',
    'loading-history': 'Loading History',
    'salesmen': 'Salesmen',
    'drivers': 'Drivers',
    'trucks': 'Trucks',
    'routes': 'Routes',
    'sales-orders': 'Sales Orders',
    'purchase-orders': 'Purchase Orders',
    'payments': 'Payments',
    'inventory': 'Inventory',
    'stock-transfers': 'Stock Transfers',
    'daily-sales-summaries': 'Daily Sales',
    'driver-settlements': 'Driver Settlements',
    'salesman-settlements': 'Salesman Settlements',
    'cylinder-deposits': 'Cylinder Deposits',
    'cylinder-exchanges': 'Cylinder Exchanges',
    'customers': 'Customers',
    'customer-cylinder-ledger': 'Cylinder Ledger',
    'customer-gas-ledger': 'Gas Ledger',
    'customer-credit': 'Credit Management',
    'advance-refills': 'Advance Refills',
    'customer-notifications': 'Notifications',
    'brands': 'Brands',
    'warehouses': 'Warehouses',
    'products': 'Products',
    'cylinders': 'Cylinders',
    'cylinder-sizes': 'Cylinder Sizes',
    'suppliers': 'Suppliers',
    'transport-companies': 'Transport Companies',
  };

  constructor(router: Router) {
    router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map((e: any) => {
        const seg = e.urlAfterRedirects || e.url;
        const path = seg.split('/').filter(Boolean)[0] || 'dashboard';
        return this.routeLabelMap[path] || 'Dashboard';
      }),
    ).subscribe(label => this.currentPageLabel.set(label));
  }
}
