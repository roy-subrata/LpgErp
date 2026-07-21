import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { FinancialReport, LowStockAlert, BrandInventory, VehicleLoading, DailySalesSummary, CashFlowEntry } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="dashboard-header">
      <div>
        <h1 class="greeting">Dashboard</h1>
        <p class="greeting-sub">{{ todayDate }} · {{ vehiclesOnRoute() }} vehicles on route · {{ lowStockCount() }} low stock alerts</p>
      </div>
      <div class="range-toggle">
        <button class="range-btn" [class.active]="activeRange() === 'today'" (click)="setRange('today')">Today</button>
        <button class="range-btn" [class.active]="activeRange() === 'week'" (click)="setRange('week')">This week</button>
        <button class="range-btn" [class.active]="activeRange() === 'month'" (click)="setRange('month')">This month</button>
      </div>
    </div>

    <!-- KPI Cards -->
    <div class="kpi-grid">
      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">Sales</span>
          <span class="kpi-icon" style="background:#fff7ed;color:#ea580c">💰</span>
        </div>
        <div class="kpi-value">৳ {{ formatMoney(financial().totalSales) }}</div>
        <div class="kpi-bottom">
          <span class="kpi-sub">{{ salesOrderCount() }} orders</span>
        </div>
      </div>
      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">Cash collected</span>
          <span class="kpi-icon" style="background:#f0fdf4;color:#15803d">💵</span>
        </div>
        <div class="kpi-value">৳ {{ formatMoney(financial().totalPayments) }}</div>
        <div class="kpi-bottom">
          <span class="kpi-sub">{{ collectionRate() }}% collection rate</span>
        </div>
      </div>
      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">Receivables</span>
          <span class="kpi-icon" style="background:#eff6ff;color:#1d4ed8">📊</span>
        </div>
        <div class="kpi-value">৳ {{ formatMoney(financial().accountsReceivable) }}</div>
        <div class="kpi-bottom">
          <span class="kpi-sub">{{ lowStockCount() }} overdue</span>
        </div>
      </div>
      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">Deposit liability</span>
          <span class="kpi-icon" style="background:#faf5ff;color:#7e22ce">🔄</span>
        </div>
        <div class="kpi-value">৳ {{ formatMoney(financial().depositLiability) }}</div>
        <div class="kpi-bottom">
          <span class="kpi-sub">pending refunds</span>
        </div>
      </div>
    </div>

    <!-- Sales Chart + Vehicles on Route -->
    <div class="row-2col">
      <div class="panel sales-chart">
        <div class="panel-header">
          <div>
            <h3 class="panel-title">Sales — {{ rangeLabel() }}</h3>
            <p class="panel-sub">Total ৳ {{ formatMoney(totalSales()) }} · Avg ৳ {{ formatMoney(avgDailySales()) }}/day</p>
          </div>
          <div class="legend">
            <span class="legend-item"><span class="legend-dot" style="background:#ea580c"></span>Cash</span>
            <span class="legend-item"><span class="legend-dot" style="background:#fdba74"></span>Credit</span>
          </div>
        </div>
        <div class="chart-bars">
          @for (bar of salesBars(); track bar.day) {
            <div class="chart-col">
              <div class="chart-bar-wrap">
                <div class="bar-segment credit" [style.height.%]="bar.creditH"></div>
                <div class="bar-segment cash" [style.height.%]="bar.cashH"></div>
              </div>
              <span class="chart-day">{{ bar.day }}</span>
            </div>
          } @empty {
            <div class="empty-chart">No sales data for this period</div>
          }
        </div>
      </div>

      <div class="panel vehicle-panel">
        <div class="panel-header">
          <h3 class="panel-title">Vehicles on route</h3>
          <a class="panel-link" routerLink="/vehicle-loadings">View all →</a>
        </div>
        <div class="vehicle-list">
          @for (v of vehicleCards(); track v.id) {
            <div class="vehicle-card">
              <div class="vehicle-top">
                <div class="vehicle-plate-row">
                  <span class="truck-icon" style="background:#fff7ed">🚛</span>
                  <span class="vehicle-plate">{{ v.plate }}</span>
                  <span class="status-pill" [style.background]="v.statusBg" [style.color]="v.statusColor">
                    <span class="status-dot" [style.background]="v.statusColor"></span>
                    {{ v.statusLabel }}
                  </span>
                </div>
                <div class="vehicle-meta">{{ v.route }} · {{ v.time }}</div>
              </div>
              <div class="vehicle-crew">
                <div class="crew-chip">
                  <span class="crew-avatar" style="background:#ea580c">{{ v.salesmanInitials }}</span>
                  <div>
                    <div class="crew-name">{{ v.salesmanName }}</div>
                    <div class="crew-role">Salesman</div>
                  </div>
                </div>
                <div class="crew-chip">
                  <span class="crew-avatar" style="background:#1d4ed8">{{ v.driverInitials }}</span>
                  <div>
                    <div class="crew-name">{{ v.driverName }}</div>
                    <div class="crew-role">Driver</div>
                  </div>
                </div>
              </div>
              <div class="vehicle-items">
                @for (item of v.items; track item.name) {
                  <div class="item-row">
                    <span class="item-name">{{ item.name }}</span>
                    <span class="item-count">{{ item.sold }}/{{ item.loaded }}</span>
                    <div class="progress-bar">
                      <div class="progress-fill" [style.width.%]="item.loaded > 0 ? (item.sold / item.loaded * 100) : 0" [style.background]="item.loaded > 0 && (item.sold / item.loaded) > 0.7 ? '#15803d' : '#ea580c'"></div>
                    </div>
                  </div>
                }
              </div>
            </div>
          } @empty {
            <div class="empty-panel">No vehicles on route today</div>
          }
        </div>
      </div>
    </div>

    <!-- Low Stock + Cylinder Stock -->
    <div class="row-2col">
      <div class="panel">
        <div class="panel-header">
          <h3 class="panel-title">Low stock alerts</h3>
        </div>
        <table class="mini-table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Warehouse</th>
              <th>Stock</th>
              <th>Minimum</th>
              <th>Level</th>
            </tr>
          </thead>
          <tbody>
            @for (alert of lowStockAlerts(); track $index) {
              <tr>
                <td>{{ alert.productName }}</td>
                <td>{{ alert.warehouseName }}</td>
                <td class="stock-val" [style.color]="alert.currentStock < alert.minimumStock / 2 ? '#dc2626' : '#a16207'">{{ alert.currentStock }}</td>
                <td>{{ alert.minimumStock }}</td>
                <td>
                  <span class="badge-pill" [style.background]="alert.currentStock < alert.minimumStock / 2 ? '#fef2f2' : '#fefce8'" [style.color]="alert.currentStock < alert.minimumStock / 2 ? '#dc2626' : '#a16207'">
                    {{ alert.currentStock < alert.minimumStock / 2 ? 'Critical' : 'Low' }}
                  </span>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="empty-cell">No low stock alerts</td></tr>
            }
          </tbody>
        </table>
      </div>

      <div class="panel">
        <div class="panel-header">
          <h3 class="panel-title">Cylinder stock by brand</h3>
        </div>
        <div class="brand-stock-list">
          @for (b of brandStock(); track b.brandName) {
            <div class="brand-row">
              <div class="brand-info">
                <span class="brand-name">{{ b.brandName }}</span>
                <span class="brand-counts">{{ b.filledCylinderCount }} filled · {{ b.emptyCylinderCount }} empty</span>
              </div>
              <div class="brand-bar-wrap">
                <div class="brand-bar">
                  <div class="brand-bar-filled" [style.width.%]="b.totalQuantity > 0 ? (b.filledCylinderCount / b.totalQuantity * 100) : 0" [style.background]="getBrandColor(b.brandName)"></div>
                  <div class="brand-bar-empty" [style.width.%]="b.totalQuantity > 0 ? (b.emptyCylinderCount / b.totalQuantity * 100) : 0" [style.background]="getBrandColor(b.brandName)" style="opacity:0.3"></div>
                </div>
              </div>
            </div>
          } @empty {
            <div class="empty-panel">No cylinder stock data</div>
          }
          <div class="brand-total">
            <span>Total cylinders</span>
            <span class="brand-total-val">{{ totalCylinders() | number }}</span>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .dashboard-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .greeting { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .greeting-sub { font-size: 13px; color: var(--text-muted); margin-top: 4px; }
    .range-toggle { display: flex; gap: 4px; background: var(--fill-subtle); border-radius: 8px; padding: 3px; border: 1px solid var(--border); }
    .range-btn { padding: 6px 14px; border: none; border-radius: 6px; background: transparent; font-size: 13px; font-weight: 500; color: var(--text-muted); cursor: pointer; }
    .range-btn.active { background: var(--sidebar-bg); color: #fff; }
    .kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 20px; }
    .kpi-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 16px 18px; }
    .kpi-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
    .kpi-label { font-size: 12px; font-weight: 600; color: var(--text-muted); }
    .kpi-icon { width: 26px; height: 26px; border-radius: 7px; display: flex; align-items: center; justify-content: center; font-size: 14px; }
    .kpi-value { font-size: 24px; font-weight: 800; letter-spacing: -0.02em; color: var(--text-primary); line-height: 1.1; }
    .kpi-bottom { display: flex; align-items: center; gap: 6px; margin-top: 6px; }
    .kpi-sub { font-size: 11px; color: var(--text-faint); }
    .row-2col { display: grid; grid-template-columns: 1.8fr 1fr; gap: 16px; margin-bottom: 20px; }
    .panel { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 18px 20px; overflow: hidden; }
    .panel-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    .panel-title { font-size: 14px; font-weight: 700; color: var(--text-primary); margin: 0; }
    .panel-sub { font-size: 12px; color: var(--text-muted); margin: 2px 0 0; }
    .panel-link { font-size: 13px; font-weight: 600; color: var(--primary); text-decoration: none; }
    .panel-link:hover { text-decoration: underline; }
    .legend { display: flex; gap: 12px; }
    .legend-item { display: flex; align-items: center; gap: 5px; font-size: 12px; color: var(--text-muted); }
    .legend-dot { width: 8px; height: 8px; border-radius: 50%; }
    .chart-bars { display: flex; align-items: flex-end; gap: 6px; height: 150px; padding-top: 10px; }
    .chart-col { flex: 1; display: flex; flex-direction: column; align-items: center; height: 100%; }
    .chart-bar-wrap { flex: 1; width: 100%; display: flex; flex-direction: column; justify-content: flex-end; gap: 1px; }
    .bar-segment { border-radius: 3px 3px 0 0; min-height: 2px; }
    .bar-segment.cash { background: #ea580c; }
    .bar-segment.credit { background: #fdba74; }
    .chart-day { font-size: 10px; color: var(--text-faint); margin-top: 4px; }
    .empty-chart, .empty-panel { display: flex; align-items: center; justify-content: center; height: 100%; min-height: 100px; color: var(--text-muted); font-size: 13px; }
    .vehicle-panel { max-height: 420px; display: flex; flex-direction: column; }
    .vehicle-list { flex: 1; overflow-y: auto; display: flex; flex-direction: column; gap: 10px; }
    .vehicle-card { background: var(--fill-subtle); border: 1px solid var(--border-row); border-radius: 10px; padding: 12px 14px; }
    .vehicle-top { margin-bottom: 8px; }
    .vehicle-plate-row { display: flex; align-items: center; gap: 8px; }
    .truck-icon { width: 32px; height: 32px; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-size: 16px; }
    .vehicle-plate { font-family: var(--font-mono); font-size: 14px; font-weight: 600; color: var(--text-primary); }
    .status-pill { display: inline-flex; align-items: center; gap: 4px; padding: 2px 8px; border-radius: 10px; font-size: 11px; font-weight: 600; margin-left: auto; }
    .status-dot { width: 6px; height: 6px; border-radius: 50%; }
    .vehicle-meta { font-size: 12px; color: var(--text-muted); margin-top: 4px; margin-left: 40px; }
    .vehicle-crew { display: flex; gap: 10px; margin-bottom: 8px; }
    .crew-chip { display: flex; align-items: center; gap: 6px; padding: 4px 10px 4px 4px; background: var(--surface); border-radius: 8px; border: 1px solid var(--border-row); }
    .crew-avatar { width: 26px; height: 26px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: #fff; font-size: 10px; font-weight: 700; }
    .crew-name { font-size: 12px; font-weight: 600; color: var(--text-primary); line-height: 1.1; }
    .crew-role { font-size: 10px; color: var(--text-muted); }
    .vehicle-items { margin-bottom: 8px; }
    .item-row { display: grid; grid-template-columns: 1fr auto 60px; gap: 8px; align-items: center; padding: 3px 0; }
    .item-name { font-size: 12px; color: var(--text-secondary); }
    .item-count { font-size: 12px; font-weight: 600; color: var(--text-primary); }
    .progress-bar { height: 5px; background: var(--border-row); border-radius: 3px; overflow: hidden; }
    .progress-fill { height: 100%; border-radius: 3px; transition: width 0.3s; }
    .mini-table { width: 100%; border-collapse: collapse; }
    .mini-table th { padding: 8px 12px; text-align: left; font-size: 11px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.04em; border-bottom: 1px solid var(--border); background: var(--fill-subtle); }
    .mini-table td { padding: 10px 12px; font-size: 13px; border-bottom: 1px solid var(--border-row); }
    .stock-val { font-weight: 700; }
    .badge-pill { display: inline-block; padding: 2px 10px; border-radius: 10px; font-size: 11px; font-weight: 600; }
    .empty-cell { text-align: center; color: var(--text-muted); padding: 30px 12px !important; }
    .brand-stock-list { display: flex; flex-direction: column; gap: 12px; }
    .brand-row { display: flex; flex-direction: column; gap: 4px; }
    .brand-info { display: flex; justify-content: space-between; align-items: center; }
    .brand-name { font-size: 13px; font-weight: 600; color: var(--text-primary); }
    .brand-counts { font-size: 12px; color: var(--text-muted); }
    .brand-bar-wrap { width: 100%; }
    .brand-bar { height: 10px; border-radius: 5px; display: flex; overflow: hidden; background: var(--fill-subtle); }
    .brand-bar-filled, .brand-bar-empty { height: 100%; }
    .brand-total { display: flex; justify-content: space-between; align-items: center; padding-top: 10px; border-top: 1px solid var(--border-row); font-size: 13px; color: var(--text-muted); }
    .brand-total-val { font-weight: 700; color: var(--text-primary); }
    @media (max-width: 1200px) { .kpi-grid { grid-template-columns: repeat(2, 1fr); } .row-2col { grid-template-columns: 1fr; } }
  `],
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);

  activeRange = signal<'today' | 'week' | 'month'>('week');
  lowStockAlerts = signal<LowStockAlert[]>([]);
  brandStock = signal<BrandInventory[]>([]);
  financial = signal<FinancialReport>({
    totalSales: 0, totalPayments: 0, totalPurchases: 0,
    totalPurchasePayments: 0, accountsReceivable: 0, supplierPayable: 0,
    transportationExpenses: 0, commissionBalance: 0, depositLiability: 0, netProfit: 0,
  });
  vehicleLoadings = signal<VehicleLoading[]>([]);
  salesOrders = signal<any[]>([]);
  cashFlowData = signal<CashFlowEntry[]>([]);

  todayDate = new Date().toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });

  lowStockCount = computed(() => this.lowStockAlerts().length);
  vehiclesOnRoute = computed(() => this.vehicleLoadings().length);
  salesOrderCount = computed(() => this.salesOrders().length);

  collectionRate = computed(() => {
    const sales = this.financial().totalSales;
    const payments = this.financial().totalPayments;
    if (sales === 0) return 0;
    return Math.round((payments / sales) * 100);
  });

  totalSales = computed(() => this.financial().totalSales);
  avgDailySales = computed(() => {
    const total = this.totalSales();
    const days = this.activeRange() === 'today' ? 1 : this.activeRange() === 'week' ? 7 : 30;
    return total / days;
  });

  rangeLabel = computed(() => {
    const r = this.activeRange();
    if (r === 'today') return 'today';
    if (r === 'week') return 'last 7 days';
    return 'this month';
  });

  vehicleCards = computed(() => {
    return this.vehicleLoadings().map(vl => {
      const items = (vl.items || []).map((item: any) => ({
        name: item.productName || item.name || 'Product',
        loaded: item.loadedQuantity || item.loaded || 0,
        sold: item.soldQuantity || item.sold || 0,
      }));
      const totalLoaded = items.reduce((sum: number, i: any) => sum + (i.loaded || 0), 0);
      const totalSold = items.reduce((sum: number, i: any) => sum + (i.sold || 0), 0);
      const statusMap: Record<number, { label: string; bg: string; color: string }> = {
        0: { label: 'Loading', bg: '#fefce8', color: '#a16207' },
        1: { label: 'Selling', bg: '#f0fdf4', color: '#15803d' },
        2: { label: 'Returned', bg: '#eff6ff', color: '#1d4ed8' },
      };
      const status = statusMap[(vl as any).status] || statusMap[0];
      const initials = (name: string) => (name || '').split(' ').map((w: string) => w[0]).slice(0, 2).join('').toUpperCase();
      return {
        id: vl.id,
        plate: vl.truckName || 'N/A',
        route: (vl as any).routeName || 'N/A',
        time: vl.loadingDate ? new Date(vl.loadingDate).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) : '',
        statusLabel: status.label,
        statusBg: status.bg,
        statusColor: status.color,
        salesmanName: vl.salesmanName || 'N/A',
        salesmanInitials: initials(vl.salesmanName),
        driverName: vl.driverName || 'N/A',
        driverInitials: initials(vl.driverName),
        items,
        totalLoaded,
        totalSold,
      };
    });
  });

  salesBars = computed(() => {
    const data = this.cashFlowData();
    if (!data.length) return [];
    const maxVal = Math.max(...data.map(d => (d.cashIn || 0) + Math.abs(d.cashOut || 0)), 1);
    return data.map(d => ({
      day: d.date ? new Date(d.date).getDate().toString() : '',
      cashH: maxVal > 0 ? ((d.cashIn || 0) / maxVal) * 100 : 0,
      creditH: maxVal > 0 ? (Math.abs(d.cashOut || 0) / maxVal) * 50 : 0,
    }));
  });

  totalCylinders = computed(() =>
    this.brandStock().reduce((sum, b) => sum + (b.totalQuantity || 0), 0)
  );

  private brandColors: Record<string, string> = {
    'Bashundhara': '#ea580c', 'Omera': '#1d4ed8', 'BM': '#0f766e',
    'Petromax': '#7e22ce', '': '#6b7280',
  };

  ngOnInit() {
    this.loadData();
  }

  setRange(range: 'today' | 'week' | 'month') {
    this.activeRange.set(range);
    this.loadData();
  }

  loadData() {
    const range = this.activeRange();
    const today = new Date();
    let from: string;

    if (range === 'today') {
      from = today.toISOString().split('T')[0];
    } else if (range === 'week') {
      const weekAgo = new Date(today);
      weekAgo.setDate(weekAgo.getDate() - 7);
      from = weekAgo.toISOString().split('T')[0];
    } else {
      const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);
      from = monthStart.toISOString().split('T')[0];
    }
    const to = today.toISOString().split('T')[0];

    forkJoin({
      financial: this.api.get<FinancialReport>('reports/financial', { from, to }),
      lowStock: this.api.getAll<LowStockAlert>('reports/inventory/low-stock', 1, 100),
      brandStock: this.api.getAll<BrandInventory>('reports/inventory/brands', 1, 100),
      vehicles: this.api.getAll<VehicleLoading>('vehicleloadings', 1, 20),
      salesOrders: this.api.getAll<any>('salesorders', 1, 100),
      cashFlow: this.api.getAll<CashFlowEntry>('reports/financial/cashflow', 1, 30, { from, to }),
    }).subscribe({
      next: (data) => {
        this.financial.set(data.financial);
        this.lowStockAlerts.set(data.lowStock.items);
        this.brandStock.set(data.brandStock.items);
        this.vehicleLoadings.set(data.vehicles.items);
        this.salesOrders.set(data.salesOrders.items);
        this.cashFlowData.set(data.cashFlow.items);
      },
    });
  }

  formatMoney(val: number): string {
    if (!val) return '0';
    if (val >= 100000) return (val / 100000).toFixed(2) + 'L';
    if (val >= 1000) return (val / 1000).toFixed(1) + 'K';
    return val.toLocaleString('en-IN');
  }

  getBrandColor(name: string): string {
    return this.brandColors[name] || '#6b7280';
  }
}
