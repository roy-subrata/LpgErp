import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { VehicleReconciliation, VehicleLoadingReport, FinancialReport } from '../../core/models';

@Component({
  selector: 'app-loading-history',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Loading History</h1>
        <p class="page-subtitle">Historical record of vehicle loadings</p>
      </div>
      <a routerLink="/vehicle-loadings" class="btn-primary">+ New Loading</a>
    </div>

    <div class="kpi-grid">
      @for (kpi of kpis(); track kpi.label) {
        <div class="kpi-card">
          <div class="kpi-label">{{ kpi.label }}</div>
          <div class="kpi-value">{{ kpi.value }}</div>
          <div class="kpi-delta" [style.color]="kpi.deltaColor">{{ kpi.delta }}</div>
        </div>
      }
    </div>

    <div class="table-card">
      <div class="toolbar">
        <input
          type="text"
          class="search-input"
          placeholder="Search vehicle, salesman..."
          [ngModel]="query()"
          (ngModelChange)="query.set($event)"
        />
        <div class="status-tabs">
          @for (tab of statusTabs; track tab) {
            <button
              class="status-tab"
              [class.active]="activeStatus() === tab"
              (click)="activeStatus.set(tab)"
            >{{ tab }}</button>
          }
        </div>
        <select class="date-dropdown" [ngModel]="dateRange()" (ngModelChange)="dateRange.set($event)">
          <option value="all">All dates</option>
          <option value="today">Today</option>
          <option value="week">This week</option>
          <option value="month">This month</option>
        </select>
        <span class="result-count">{{ filteredItems().length }} results</span>
      </div>

      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th style="width:110px">Vehicle</th>
              <th>Date</th>
              <th>Salesman / Route</th>
              <th style="width:70px">Loaded</th>
              <th style="width:80px">Sold</th>
              <th style="width:90px">Returned</th>
              <th style="width:110px">Cash</th>
              <th style="width:100px">Status</th>
              <th style="width:60px"></th>
            </tr>
          </thead>
          <tbody>
            @for (row of pagedItems(); track row.id) {
              <tr>
                <td class="mono">{{ row.truckName }}</td>
                <td>{{ row.dateFormatted }}</td>
                <td>{{ row.salesmanName }} / {{ row.warehouseName }}</td>
                <td class="num">{{ row.totalLoaded }}</td>
                <td class="num">{{ row.totalSold }}</td>
                <td class="num">{{ row.totalReturned }}</td>
                <td class="num cash">৳ {{ row.cashCollected | number:'1.0-0' }}</td>
                <td>
                  <span class="status-badge" [ngClass]="'badge-' + row.status.toLowerCase()">
                    {{ row.status }}
                  </span>
                </td>
                <td class="action-cell">
                  <button class="view-btn" [routerLink]="row.loadingId ? ['/vehicle-loadings', row.loadingId] : ['/vehicle-loadings']">→</button>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="9" class="empty-cell">No loading records found.</td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <div class="pagination">
        <span class="page-info">Showing {{ pageStart() }}–{{ pageEnd() }} of {{ filteredItems().length }}</span>
        <div class="page-controls">
          <button class="page-btn" [disabled]="currentPage() === 1" (click)="currentPage.set(currentPage() - 1)">← Prev</button>
          @for (p of pageNumbers(); track p) {
            <button class="page-btn num-btn" [class.active]="currentPage() === p" (click)="currentPage.set(p)">{{ p }}</button>
          }
          <button class="page-btn" [disabled]="currentPage() === totalPages()" (click)="currentPage.set(currentPage() + 1)">Next →</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .page-subtitle { font-size: 13px; color: var(--text-muted); margin: 4px 0 0; }
    .btn-primary { display: inline-flex; align-items: center; gap: 4px; background: var(--primary); color: #fff; border: none; padding: 8px 16px; border-radius: var(--radius-btn); font-size: 13px; font-weight: 600; cursor: pointer; text-decoration: none; transition: background 0.15s; box-shadow: var(--shadow-btn); }
    .btn-primary:hover { background: var(--primary-hover); text-decoration: none; color: #fff; }
    .kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 20px; }
    .kpi-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 16px; }
    .kpi-label { font-size: 12px; font-weight: 600; color: var(--text-muted); margin-bottom: 6px; }
    .kpi-value { font-size: 22px; font-weight: 800; color: var(--text-primary); line-height: 1.1; }
    .kpi-delta { font-size: 11px; font-weight: 600; margin-top: 4px; }
    .table-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); overflow: hidden; }
    .toolbar { display: flex; align-items: center; gap: 12px; padding: 14px 18px; border-bottom: 1px solid var(--border-row); flex-wrap: wrap; }
    .search-input { width: 240px; padding: 7px 12px; border: 1px solid var(--border-input); border-radius: var(--radius-btn); font-size: 13px; color: var(--text-primary); background: var(--surface); outline: none; transition: border-color 0.15s; }
    .search-input:focus { border-color: var(--primary); }
    .search-input::placeholder { color: var(--text-faint); }
    .status-tabs { display: flex; gap: 4px; background: var(--fill-subtle); border-radius: var(--radius-pill); padding: 3px; border: 1px solid var(--border); }
    .status-tab { padding: 5px 14px; border: none; border-radius: var(--radius-pill); background: transparent; font-size: 12px; font-weight: 600; color: var(--text-muted); cursor: pointer; transition: all 0.15s; }
    .status-tab.active { background: var(--surface); color: var(--text-primary); box-shadow: 0 1px 2px rgba(0,0,0,0.06); }
    .date-dropdown { padding: 7px 10px; border: 1px solid var(--border-input); border-radius: var(--radius-btn); font-size: 13px; color: var(--text-primary); background: var(--surface); outline: none; cursor: pointer; }
    .result-count { margin-left: auto; font-size: 12px; color: var(--text-faint); font-weight: 500; }
    .table-wrap { overflow-x: auto; min-width: 980px; }
    table { width: 100%; border-collapse: collapse; }
    thead tr { background: var(--fill-subtle); }
    th { padding: 9px 14px; text-align: left; font-size: 11px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.04em; border-bottom: 1px solid var(--border); white-space: nowrap; }
    td { padding: 10px 14px; font-size: 13px; color: var(--text-secondary); border-bottom: 1px solid var(--border-row); }
    tbody tr:hover { background: var(--fill-subtle2); }
    .mono { font-family: var(--font-mono); font-size: 12px; font-weight: 600; color: var(--text-primary); }
    .num { text-align: center; font-weight: 600; color: var(--text-primary); }
    .cash { font-weight: 700; color: var(--green-fg); }
    .action-cell { text-align: center; }
    .view-btn { width: 28px; height: 28px; border: 1px solid var(--border); border-radius: var(--radius-btn); background: var(--surface); color: var(--text-muted); cursor: pointer; font-size: 13px; display: inline-flex; align-items: center; justify-content: center; transition: all 0.15s; text-decoration: none; }
    .view-btn:hover { border-color: var(--primary); color: var(--primary); background: var(--primary-tint1); }
    .empty-cell { text-align: center; color: var(--text-muted); padding: 40px 14px !important; }
    .status-badge { display: inline-block; padding: 2px 10px; border-radius: 10px; font-size: 11px; font-weight: 600; white-space: nowrap; }
    .badge-loading { background: #fefce8; color: #a16207; }
    .badge-selling { background: #f0fdf4; color: #15803d; }
    .badge-closed, .badge-returned { background: #eff6ff; color: #1d4ed8; }
    .badge-cancelled { background: #fef2f2; color: #dc2626; }
    .pagination { display: flex; align-items: center; justify-content: space-between; padding: 12px 18px; border-top: 1px solid var(--border-row); }
    .page-info { font-size: 12px; color: var(--text-faint); }
    .page-controls { display: flex; align-items: center; gap: 4px; }
    .page-btn { padding: 5px 12px; border: 1px solid var(--border); border-radius: var(--radius-btn); background: var(--surface); font-size: 12px; font-weight: 600; color: var(--text-muted); cursor: pointer; transition: all 0.15s; }
    .page-btn:hover:not(:disabled):not(.active) { border-color: var(--primary); color: var(--primary); }
    .page-btn:disabled { opacity: 0.4; cursor: not-allowed; }
    .page-btn.active { background: var(--primary); border-color: var(--primary); color: #fff; }
    @media (max-width: 900px) { .kpi-grid { grid-template-columns: repeat(2, 1fr); } .toolbar { flex-direction: column; align-items: stretch; } .search-input { width: 100%; } .result-count { margin-left: 0; } }
  `],
})
export class LoadingHistoryComponent implements OnInit {
  private api = inject(ApiService);

  readonly PAGE_SIZE = 5;

  query = signal('');
  activeStatus = signal<string>('All');
  currentPage = signal(1);
  dateRange = signal('all');

  statusTabs = ['All', 'Selling', 'Closed', 'Cancelled'];

  allItems = signal<any[]>([]);

  kpis = computed(() => {
    const items = this.allItems();
    const loadingCount = items.filter(i => i.status === 'Loading').length;
    const sellingCount = items.filter(i => i.status === 'Selling').length;
    const closedCount = items.filter(i => i.status === 'Closed' || i.status === 'Returned').length;
    const totalLoaded = items.reduce((s, i) => s + (i.totalLoaded || 0), 0);
    const totalSold = items.reduce((s, i) => s + (i.totalSold || 0), 0);
    const totalCash = items.reduce((s, i) => s + (i.cashCollected || 0), 0);
    const sellThrough = totalLoaded > 0 ? Math.round((totalSold / totalLoaded) * 100) : 0;
    return [
      { label: 'Total loadings', value: String(items.length), delta: `${sellingCount} active`, deltaColor: '#15803d' },
      { label: 'Cylinders loaded', value: String(totalLoaded), delta: `${totalSold} sold total`, deltaColor: '#15803d' },
      { label: 'Sell-through', value: sellThrough + '%', delta: `${closedCount} closed`, deltaColor: '#15803d' },
      { label: 'Cash collected', value: '৳ ' + this.formatMoney(totalCash), delta: '', deltaColor: '#15803d' },
    ];
  });

  filteredItems = computed(() => {
    const q = this.query().toLowerCase();
    const status = this.activeStatus();
    const range = this.dateRange();

    return this.allItems().filter(row => {
      if (q && !(row.truckName?.toLowerCase().includes(q) || row.salesmanName?.toLowerCase().includes(q))) return false;
      if (status !== 'All' && row.status !== status) return false;
      if (range !== 'all') {
        const rowDate = new Date(row.date);
        const now = new Date();
        if (range === 'today') {
          const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
          if (rowDate < today) return false;
        } else if (range === 'week') {
          const weekAgo = new Date(now); weekAgo.setDate(weekAgo.getDate() - 7);
          if (rowDate < weekAgo) return false;
        } else if (range === 'month') {
          const monthAgo = new Date(now); monthAgo.setMonth(monthAgo.getMonth() - 1);
          if (rowDate < monthAgo) return false;
        }
      }
      return true;
    });
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.filteredItems().length / this.PAGE_SIZE)));

  pagedItems = computed(() => {
    const page = this.currentPage();
    const start = (page - 1) * this.PAGE_SIZE;
    return this.filteredItems().slice(start, start + this.PAGE_SIZE);
  });

  pageStart = computed(() => {
    if (this.filteredItems().length === 0) return 0;
    return (this.currentPage() - 1) * this.PAGE_SIZE + 1;
  });

  pageEnd = computed(() => Math.min(this.currentPage() * this.PAGE_SIZE, this.filteredItems().length));

  pageNumbers = computed(() => Array.from({ length: this.totalPages() }, (_, i) => i + 1));

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    const now = new Date();
    const from = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0];
    const to = now.toISOString().split('T')[0];

    forkJoin({
      recon: this.api.get<any[]>('reports/vehicles/reconciliation', { from, to }),
      loading: this.api.get<any[]>('reports/vehicles', { from, to }),
      financial: this.api.get<FinancialReport>('reports/financial', { from, to }),
    }).subscribe(data => {
      const statusMap: Record<string, string> = { Loading: 'Loading', Selling: 'Selling', Returned: 'Closed', Closed: 'Closed', Cancelled: 'Cancelled' };
      const items = data.recon.map((r: any) => ({
        id: r.truckName + r.date,
        loadingId: r.vehicleLoadingId,
        truckName: r.truckName,
        date: r.date,
        dateFormatted: new Date(r.date).toLocaleDateString('en-US', { day: 'numeric', month: 'short', year: 'numeric' }),
        salesmanName: r.salesmanName,
        warehouseName: '',
        totalLoaded: r.totalLoaded,
        totalSold: r.totalSold,
        totalReturned: r.totalReturned,
        cashCollected: r.cashCollected,
        creditSales: r.creditSales,
        status: r.totalReturned > 0 ? 'Closed' : (r.totalSold > 0 ? 'Selling' : 'Loading'),
      }));

      if (items.length === 0 && data.loading.length > 0) {
        const mapped = data.loading.map((v: any) => ({
          id: v.id,
          loadingId: v.id,
          truckName: v.truckName,
          date: v.date,
          dateFormatted: new Date(v.date).toLocaleDateString('en-US', { day: 'numeric', month: 'short', year: 'numeric' }),
          salesmanName: v.salesmanName,
          warehouseName: v.warehouseName,
          totalLoaded: v.itemCount || 0,
          totalSold: 0,
          totalReturned: 0,
          cashCollected: 0,
          creditSales: 0,
          status: v.status || 'Loading',
        }));
        this.allItems.set(mapped);
      } else {
        this.allItems.set(items);
      }
    });
  }

  formatMoney(val: number): string {
    if (!val) return '0';
    if (val >= 100000) return (val / 100000).toFixed(2) + 'L';
    if (val >= 1000) return (val / 1000).toFixed(1) + 'K';
    return val.toLocaleString('en-IN');
  }
}
