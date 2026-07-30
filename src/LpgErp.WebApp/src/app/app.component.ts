import { Component, signal, computed, effect, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { filter, map } from 'rxjs';
import { AuthService } from './core/auth.service';
import { NotificationHubService } from './core/notification-hub.service';
import { CompanySettingsService } from './core/company-settings.service';
import { ToastComponent } from './features/toast/toast.component';
import { NotificationPanelComponent } from './shared/notification-panel.component';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  /** Hidden from the sidebar for anyone without the Admin role — the route itself is guarded too. */
  adminOnly?: boolean;
}

interface NavGroup {
  title: string;
  items: NavItem[];
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ToastComponent, NotificationPanelComponent],
  template: `
    @if (showShell()) {
    <div class="app-shell">
      <nav class="sidebar" [class.collapsed]="sidebarCollapsed()">
        <div class="sidebar-header">
          <div class="brand-logo">L</div>
          @if (!sidebarCollapsed()) {
            <div class="brand-text">
              <div class="brand-title">{{ companySettings.displayName() }}</div>
              <div class="brand-subtitle">DISTRIBUTOR SUITE</div>
            </div>
          }
        </div>

        <div class="nav-body">
          @for (group of navGroups; track group.title) {
            @if (groupVisible(group)) {
            @if (!sidebarCollapsed()) {
              <div class="nav-group-label">{{ group.title }}</div>
            } @else {
              <div class="nav-group-divider"></div>
            }
            @for (item of group.items; track item.route) {
              @if (!item.adminOnly || isAdmin()) {
                <a class="nav-item" [routerLink]="item.route" routerLinkActive="active" [title]="item.label">
                  <span class="nav-icon">{{ item.icon }}</span>
                  @if (!sidebarCollapsed()) {
                    <span class="nav-label">{{ item.label }}</span>
                  }
                </a>
              }
            }
            }
          }
        </div>

        <div class="sidebar-footer">
          <div class="user-avatar">{{ userInitials() }}</div>
          @if (!sidebarCollapsed()) {
            <div class="user-info">
              <div class="user-name">{{ userName() }}</div>
              <div class="user-role">{{ userRole() }}</div>
            </div>
            <button class="logout-btn" (click)="logout()" title="Sign out">⏻</button>
          }
        </div>
      </nav>

      <div class="main-area">
        <header class="topbar">
          <div class="topbar-left">
            <button class="toggle-btn" (click)="toggleSidebar()" [title]="sidebarCollapsed() ? 'Expand sidebar' : 'Collapse sidebar'">
              <span class="toggle-icon">{{ sidebarCollapsed() ? '☰' : '✕' }}</span>
            </button>
            <div class="breadcrumb">
              <span class="breadcrumb-muted">{{ companySettings.displayName() }}</span>
              <span class="breadcrumb-sep">/</span>
              <span class="breadcrumb-active">{{ currentPageLabel() }}</span>
            </div>
          </div>
          <div class="topbar-right">
            <div class="search-pill">
              <span class="search-icon">🔍</span>
              <input type="text" placeholder="Search..." class="search-input" />
              <span class="search-shortcut">⌘K</span>
            </div>
            <div class="bell-wrap">
              <button class="bell-btn" (click)="toggleNotifications($event)">
                🔔
                @if (unreadCount() > 0) {
                  <span class="bell-badge">{{ unreadCount() }}</span>
                }
              </button>
              <app-notification-panel [open]="notificationsOpen()" (openChange)="notificationsOpen.set($event)" />
            </div>
          </div>
        </header>

        <main class="content">
          <router-outlet />
        </main>
      </div>
    </div>
    } @else {
      <router-outlet />
    }
    <app-toast />
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
      transition: width 0.2s ease, min-width 0.2s ease;
    }

    .sidebar.collapsed {
      width: 62px;
      min-width: 62px;
    }

    .sidebar-header {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 18px 18px 16px;
      border-bottom: 1px solid var(--sidebar-divider);
    }

    .sidebar.collapsed .sidebar-header {
      justify-content: center;
      padding: 18px 0 16px;
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

    .sidebar.collapsed .nav-body {
      padding: 8px 0;
    }

    .nav-group-label {
      font-size: 10px;
      font-weight: 700;
      color: var(--sidebar-label);
      text-transform: uppercase;
      letter-spacing: 0.12em;
      padding: 14px 10px 5px;
    }

    .nav-group-divider {
      height: 1px;
      background: var(--sidebar-divider);
      margin: 6px 8px;
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

    .sidebar.collapsed .nav-item {
      justify-content: center;
      padding: 7px 0;
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

    .sidebar.collapsed .sidebar-footer {
      justify-content: center;
      padding: 12px 0;
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

    .logout-btn {
      margin-left: auto;
      width: 28px;
      height: 28px;
      border-radius: 6px;
      border: 1px solid var(--sidebar-divider);
      background: transparent;
      color: var(--sidebar-muted);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 14px;
      transition: background 0.15s, color 0.15s;
    }

    .logout-btn:hover {
      background: rgba(239, 68, 68, 0.15);
      color: #ef4444;
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

    .topbar-left {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .toggle-btn {
      width: 34px;
      height: 34px;
      border-radius: 8px;
      border: 1px solid var(--border);
      background: var(--surface);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: background 0.15s;
    }
    .toggle-btn:hover {
      background: var(--fill-subtle);
    }
    .toggle-icon {
      font-size: 16px;
      color: var(--text-secondary);
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

    .bell-wrap {
      position: relative;
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

    .bell-badge {
      position: absolute;
      top: 4px;
      right: 4px;
      min-width: 16px;
      height: 16px;
      background: #ef4444;
      color: #fff;
      border-radius: 8px;
      font-size: 10px;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 0 4px;
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
  private authService = inject(AuthService);
  private notificationHub = inject(NotificationHubService);
  private titleService = inject(Title);
  companySettings = inject(CompanySettingsService);

  sidebarCollapsed = signal(false);
  private onLoginRoute = signal(false);

  // The sidebar and topbar are chrome for signed-in users. The login page is a
  // top-level route sharing this outlet, so it has to render bare.
  showShell = computed(() => this.authService.isAuthenticated() && !this.onLoginRoute());

  // Derived from the auth signal rather than read once at startup — this component
  // is the root and is never re-created, so after login there is no second ngOnInit
  // to pick the user up.
  userName = computed(() => {
    const user = this.authService.currentUser();
    return user ? user.fullName || user.username : '';
  });
  userInitials = computed(() =>
    this.userName().split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
  );
  userRole = computed(() => this.authService.currentUser()?.roles[0] || 'User');
  unreadCount = this.notificationHub.unreadCount;
  notificationsOpen = signal(false);
  isAdmin = computed(() => this.authService.currentUser()?.roles.includes('Admin') ?? false);

  // Hides a group entirely once every item in it is admin-only and the viewer isn't one —
  // otherwise "Administration" would show as an empty heading with nothing underneath it.
  groupVisible(group: NavGroup): boolean {
    return group.items.some(item => !item.adminOnly || this.isAdmin());
  }

  constructor(router: Router) {
    this.companySettings.load();
    // Browser tab title follows the configured company name once it loads; falls back to the
    // "LPG ERP" default from CompanySettingsService.displayName() until then.
    effect(() => {
      this.titleService.setTitle(`${this.companySettings.displayName()} — Distributor Suite`);
    });

    // Connect once the user is authenticated, which for a fresh login happens long
    // after construction. start() is a no-op when already connected.
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.notificationHub.start();
      }
    });

    router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map((e: any) => {
        const url: string = e.urlAfterRedirects || e.url;
        return url.split(/[?#]/)[0].split('/').filter(Boolean)[0] || 'dashboard';
      }),
    ).subscribe(path => {
      this.onLoginRoute.set(path === 'login');
      this.currentPageLabel.set(this.routeLabelMap[path] || 'Dashboard');
    });
  }

  toggleNotifications(event: MouseEvent) {
    event.stopPropagation();
    this.notificationsOpen.update(v => !v);
  }

  logout() {
    this.notificationHub.stop();
    this.authService.logout();
  }

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
        { label: 'Goods Receipt', icon: '📥', route: 'goods-receipt' },
        { label: 'Payments', icon: '৳', route: 'payments' },
        { label: 'Payment Accounts', icon: '🏦', route: 'payment-accounts' },
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
        { label: 'Price History', icon: '📈', route: 'price-histories' },
      ],
    },
    {
      title: 'Commission',
      items: [
        { label: 'Commission Policies', icon: '📋', route: 'commission-policies' },
        { label: 'Commission Ledger', icon: '☰', route: 'commission-ledgers' },
      ],
    },
    {
      title: 'Administration',
      items: [
        { label: 'Users', icon: '🔐', route: 'users', adminOnly: true },
        { label: 'Roles & Permissions', icon: '🛡', route: 'roles', adminOnly: true },
        { label: 'Company Settings', icon: '⚙', route: 'settings', adminOnly: true },
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
    'goods-receipt': 'Goods Receipt',
    'payments': 'Payments',
    'payment-accounts': 'Payment Accounts',
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
    'commission-policies': 'Commission Policies',
    'commission-ledgers': 'Commission Ledger',
    'users': 'Users',
    'roles': 'Roles & Permissions',
    'settings': 'Company Settings',
  };

  toggleSidebar() {
    this.sidebarCollapsed.update(v => !v);
  }
}
