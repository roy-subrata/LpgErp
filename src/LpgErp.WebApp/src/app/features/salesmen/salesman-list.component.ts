import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { EntityDrawerComponent, DrawerField } from '../../shared/entity-drawer.component';
import { FinancialReport } from '../../core/models';

@Component({
  selector: 'app-salesman-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, EntityDrawerComponent],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Salesmen</h1>
      </div>
      <div class="header-actions">
        <button class="btn-secondary-sm" (click)="onExport()">↓ Export</button>
        <button class="btn-primary-sm" (click)="openNew()">+ New Salesman</button>
      </div>
    </div>

    <div class="kpi-grid">
      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">Active Salesmen</span>
          <span class="kpi-icon" style="background:#f0fdf4;color:#15803d">👤</span>
        </div>
        <div class="kpi-value">{{ activeSalesmenCount() }}</div>
      </div>
      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">Total Sales</span>
          <span class="kpi-icon" style="background:#eff6ff;color:#1d4ed8">৳</span>
        </div>
        <div class="kpi-value">৳{{ formatMoney(financial().totalSales) }}</div>
      </div>
      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">Cash Collected</span>
          <span class="kpi-icon" style="background:#f0fdf4;color:#15803d">✓</span>
        </div>
        <div class="kpi-value">৳{{ formatMoney(financial().totalPayments) }}</div>
        <div class="kpi-bottom">
          <span class="kpi-delta" style="color:#15803d">{{ collectionRate() }}%</span>
          <span class="kpi-sub">collection rate</span>
        </div>
      </div>
      <div class="kpi-card">
        <div class="kpi-top">
          <span class="kpi-label">Outstanding Due</span>
          <span class="kpi-icon" style="background:#fefce8;color:#a16207">!</span>
        </div>
        <div class="kpi-value">৳{{ formatMoney(financial().accountsReceivable) }}</div>
      </div>
    </div>

    <div class="table-card">
      <div class="table-toolbar">
        <div class="search-box">
          <input type="text" class="search-input" placeholder="Search salesmen..." [ngModel]="query()" (ngModelChange)="query.set($event)" />
        </div>
        <div class="toolbar-right">
          <div class="tab-group">
            <button class="tab-btn" [class.active]="activeTab() === ''" (click)="activeTab.set(''); currentPage.set(1)">All</button>
            @for (tab of tabs; track tab.value) {
              <button class="tab-btn" [class.active]="activeTab() === tab.value" (click)="activeTab.set(tab.value); currentPage.set(1)">{{ tab.label }}</button>
            }
          </div>
          <span class="result-count">{{ filteredItems().length }} results</span>
        </div>
      </div>

      <div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width:30%">Salesman</th>
              <th style="width:15%">Code</th>
              <th style="width:20%">Phone</th>
              <th style="width:12%">Status</th>
              <th class="actions-col"></th>
            </tr>
          </thead>
          <tbody>
            @for (item of pagedItems(); track item.id || $index) {
              <tr class="data-row" (click)="openView(item)">
                <td><span class="main-text">{{ item.name }}</span></td>
                <td><span class="mono-text">{{ item.code }}</span></td>
                <td><span class="muted-text">{{ item.phone }}</span></td>
                <td>
                  <span class="badge" [style.background]="item.isActive ? '#f0fdf4' : '#f4f5f7'" [style.color]="item.isActive ? '#15803d' : '#6b7280'">{{ item.isActive ? 'Active' : 'Inactive' }}</span>
                </td>
                <td class="actions-col">
                  <button class="action-btn" title="View" (click)="openView(item); $event.stopPropagation()">→</button>
                  <button class="action-btn" title="Edit" (click)="openEdit(item); $event.stopPropagation()">✎</button>
                  <button class="action-btn danger" title="Delete" (click)="onDelete(item); $event.stopPropagation()">🗑</button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="empty-row">No salesmen found.</td></tr>
            }
          </tbody>
        </table>
      </div>

      @if (totalPages() > 1) {
        <div class="table-footer">
          <span class="footer-info">Showing {{ (currentPage() - 1) * pageSize + 1 }}–{{ Math.min(currentPage() * pageSize, filteredItems().length) }} of {{ filteredItems().length }}</span>
          <div class="pagination">
            <button class="page-btn" [disabled]="currentPage() === 1" (click)="currentPage.set(currentPage() - 1)">←</button>
            @for (p of pageNumbers(); track p) {
              <button class="page-btn" [class.active]="currentPage() === p" (click)="currentPage.set(p)">{{ p }}</button>
            }
            <button class="page-btn" [disabled]="currentPage() === totalPages()" (click)="currentPage.set(currentPage() + 1)">→</button>
          </div>
        </div>
      }
    </div>

    <app-entity-drawer
      [open]="drawerOpen()"
      [title]="drawerTitle()"
      [subtitle]="drawerSubtitle()"
      [mode]="drawerMode()"
      [fields]="drawerFields"
      [entity]="currentEntity()"
      [saving]="saving()"
      (closeDrawer)="drawerOpen.set(false)"
      (saveEntity)="onSave($event)"
    />
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .header-actions { display: flex; gap: 8px; align-items: center; }
    .btn-primary-sm { padding: 9px 18px; border-radius: 7px; border: none; background: var(--primary); color: #fff; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: var(--shadow-btn); }
    .btn-primary-sm:hover { background: var(--primary-hover); }
    .btn-secondary-sm { padding: 9px 18px; border-radius: 7px; border: 1px solid var(--border-input); background: var(--surface); color: var(--text-secondary); font-size: 13px; font-weight: 600; cursor: pointer; }
    .btn-secondary-sm:hover { background: var(--fill-subtle); }
    .kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 20px; }
    .kpi-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 16px 18px; }
    .kpi-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
    .kpi-label { font-size: 12px; font-weight: 600; color: var(--text-muted); }
    .kpi-icon { width: 26px; height: 26px; border-radius: 7px; display: flex; align-items: center; justify-content: center; font-size: 14px; }
    .kpi-value { font-size: 24px; font-weight: 800; letter-spacing: -0.02em; color: var(--text-primary); line-height: 1.1; }
    .kpi-bottom { display: flex; align-items: center; gap: 6px; margin-top: 6px; }
    .kpi-delta { font-size: 12px; font-weight: 600; }
    .kpi-sub { font-size: 11px; color: var(--text-faint); }
    .table-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); overflow: hidden; }
    .table-toolbar { display: flex; align-items: center; justify-content: space-between; padding: 12px 16px; border-bottom: 1px solid var(--border-row); gap: 12px; flex-wrap: wrap; }
    .search-box { width: 240px; }
    .search-input { width: 100%; padding: 8px 12px; border: 1px solid var(--border-input); border-radius: 7px; font-size: 13px; outline: none; background: var(--surface); }
    .search-input:focus { border-color: var(--primary); box-shadow: 0 0 0 3px rgba(234,88,12,0.12); }
    .toolbar-right { display: flex; align-items: center; gap: 12px; }
    .tab-group { display: flex; gap: 4px; background: var(--fill-subtle); border-radius: 7px; padding: 3px; }
    .tab-btn { padding: 5px 12px; border: none; border-radius: 5px; background: transparent; font-size: 12px; font-weight: 600; color: var(--text-muted); cursor: pointer; transition: all 0.15s; }
    .tab-btn.active { background: var(--surface); color: var(--text-primary); box-shadow: 0 1px 2px rgba(0,0,0,0.06); }
    .tab-btn:hover:not(.active) { color: var(--text-secondary); }
    .result-count { font-size: 12px; color: var(--text-muted); }
    .table-scroll { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; min-width: 500px; }
    .data-table th { padding: 10px 14px; text-align: left; font-size: 12px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.04em; border-bottom: 1px solid var(--border); background: var(--fill-subtle); white-space: nowrap; }
    .data-table td { padding: 12px 14px; font-size: 13px; border-bottom: 1px solid var(--border-row); color: var(--text-primary); }
    .data-row { cursor: pointer; transition: background 0.1s; }
    .data-row:hover { background: var(--fill-subtle); }
    .main-text { font-weight: 600; color: var(--text-primary); }
    .mono-text { font-family: var(--font-mono); font-size: 12px; color: var(--text-muted); }
    .muted-text { font-size: 12px; color: var(--text-muted); }
    .badge { display: inline-block; padding: 3px 10px; border-radius: var(--radius-pill); font-size: 12px; font-weight: 600; white-space: nowrap; }
    .actions-col { width: 80px; text-align: center; }
    .action-btn { width: 28px; height: 28px; border: 1px solid var(--border); border-radius: 5px; background: var(--surface); cursor: pointer; font-size: 13px; margin: 0 2px; display: inline-flex; align-items: center; justify-content: center; }
    .action-btn:hover { background: var(--fill-subtle); }
    .action-btn.danger { color: var(--red-fg); border-color: var(--red-bg); }
    .action-btn.danger:hover { background: var(--red-bg); }
    .empty-row { text-align: center; padding: 40px 14px !important; color: var(--text-muted); }
    .table-footer { display: flex; align-items: center; justify-content: space-between; padding: 12px 16px; border-top: 1px solid var(--border-row); }
    .footer-info { font-size: 12px; color: var(--text-muted); }
    .pagination { display: flex; gap: 4px; }
    .page-btn { width: 30px; height: 30px; border: 1px solid var(--border); border-radius: 5px; background: var(--surface); cursor: pointer; font-size: 12px; font-weight: 600; display: flex; align-items: center; justify-content: center; color: var(--text-secondary); }
    .page-btn.active { background: var(--primary); color: #fff; border-color: var(--primary); }
    .page-btn:hover:not(.active):not(:disabled) { background: var(--fill-subtle); }
    .page-btn:disabled { opacity: 0.4; cursor: not-allowed; }
    @media (max-width: 768px) { .kpi-grid { grid-template-columns: repeat(2, 1fr); } }
  `],
})
export class SalesmanListComponent implements OnInit {
  private api = inject(ApiService);

  Math = Math;
  pageSize = 15;

  items = signal<any[]>([]);
  financial = signal<FinancialReport>({ totalSales: 0, totalPayments: 0, totalPurchases: 0, totalPurchasePayments: 0, accountsReceivable: 0, supplierPayable: 0, transportationExpenses: 0, commissionBalance: 0, depositLiability: 0, netProfit: 0 });
  query = signal('');
  activeTab = signal('');
  currentPage = signal(1);
  drawerOpen = signal(false);
  drawerMode = signal<'view' | 'edit' | 'new'>('new');
  currentEntity = signal<any>(null);
  saving = signal(false);

  readonly tabs = [
    { label: 'Active', value: 'true' },
    { label: 'Inactive', value: 'false' },
  ];

  readonly drawerFields: DrawerField[] = [
    { key: 'name', label: 'Name', type: 'text', required: true },
    { key: 'code', label: 'Code', type: 'text', required: true, mono: true },
    { key: 'phone', label: 'Phone', type: 'text' },
    { key: 'isActive', label: 'Active', type: 'toggle' },
  ];

  activeSalesmenCount = computed(() => this.items().filter(i => i.isActive).length);
  collectionRate = computed(() => {
    const sales = this.financial().totalSales;
    const payments = this.financial().totalPayments;
    return sales > 0 ? Math.round((payments / sales) * 100) : 0;
  });

  filteredItems = computed(() => {
    let list = this.items();
    const tab = this.activeTab();
    const q = this.query().toLowerCase();
    if (tab) list = list.filter(i => String(i.isActive) === tab);
    if (q) list = list.filter(i => ['name', 'code', 'phone'].some(f => String(i[f] ?? '').toLowerCase().includes(q)));
    return list;
  });

  totalPages = computed(() => Math.ceil(this.filteredItems().length / this.pageSize));

  pagedItems = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.filteredItems().slice(start, start + this.pageSize);
  });

  pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    for (let i = Math.max(1, current - 2); i <= Math.min(total, current + 2); i++) pages.push(i);
    return pages;
  });

  drawerTitle = computed(() => {
    const mode = this.drawerMode();
    if (mode === 'new') return 'New Salesman';
    if (mode === 'edit') return 'Edit Salesman';
    return 'Salesman';
  });

  drawerSubtitle = computed(() => {
    const entity = this.currentEntity();
    if (entity?.id) return entity.id.substring(0, 8);
    return '';
  });

  ngOnInit() {
    this.load();
    this.api.get<FinancialReport>('reports/financial', { from: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0], to: new Date().toISOString().split('T')[0] }).subscribe(f => this.financial.set(f));
  }

  load() {
    this.api.getAllList<any>('salesmen').subscribe(data => this.items.set(data));
  }

  openNew() { this.currentEntity.set({}); this.drawerMode.set('new'); this.drawerOpen.set(true); }
  openView(item: any) { this.currentEntity.set({ ...item }); this.drawerMode.set('view'); this.drawerOpen.set(true); }
  openEdit(item: any) { this.currentEntity.set({ ...item }); this.drawerMode.set('edit'); this.drawerOpen.set(true); }

  onSave(data: any) {
    this.saving.set(true);
    const id = data.id || data.Id;
    const req$ = id ? this.api.update('salesmen', id, data) : this.api.create('salesmen', data);
    req$.subscribe({ next: () => { this.saving.set(false); this.drawerOpen.set(false); this.load(); }, error: () => this.saving.set(false) });
  }

  onDelete(item: any) {
    if (confirm('Delete this salesman?')) {
      this.api.delete('salesmen', item.id || item.Id).subscribe(() => this.load());
    }
  }

  onExport() {}

  formatMoney(val: number): string {
    if (!val) return '0';
    if (val >= 100000) return (val / 100000).toFixed(2) + 'L';
    if (val >= 1000) return (val / 1000).toFixed(1) + 'K';
    return val.toLocaleString('en-IN');
  }
}
