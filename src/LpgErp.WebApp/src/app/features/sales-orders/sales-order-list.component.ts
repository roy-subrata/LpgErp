import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { SalesOrder } from '../../core/models';
import { EntityDrawerComponent, DrawerField } from '../../shared/entity-drawer.component';

@Component({
  selector: 'app-sales-order-list',
  standalone: true,
  imports: [CommonModule, FormsModule, EntityDrawerComponent],
  template: `
    <div class="page-header">
      <h1 class="page-title">Sales Orders</h1>
      <div class="header-actions">
        <button class="btn-secondary">↓ Export</button>
        <button class="btn-primary" (click)="openNew()">+ New Order</button>
      </div>
    </div>

    <!-- Status Pipeline -->
    <div class="pipeline-grid">
      @for (pipe of pipelines(); track pipe.label) {
        <div class="pipeline-card" [class.active]="activeStatus() === pipe.value" (click)="toggleStatus(pipe.value)">
          <div class="pipe-header">
            <span class="pipe-dot" [style.background]="pipe.dotColor"></span>
            <span class="pipe-label">{{ pipe.label }}</span>
          </div>
          <div class="pipe-count">{{ pipe.count }}</div>
          <div class="pipe-sub">{{ pipe.sub }}</div>
        </div>
      }
    </div>

    <!-- Table Card -->
    <div class="table-card">
      <div class="table-toolbar">
        <div class="search-box">
          <input type="text" class="search-input" placeholder="Search orders..." [ngModel]="query()" (ngModelChange)="query.set($event)" />
        </div>
        <div class="toolbar-right">
          <span class="result-count">{{ filteredItems().length }} results</span>
        </div>
      </div>

      <div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width:110px">Order #</th>
              <th style="min-width:170px">Customer</th>
              <th style="min-width:150px">Items</th>
              <th style="width:90px">Payment</th>
              <th style="min-width:100px">Amount</th>
              <th style="min-width:90px">Due</th>
              <th style="width:100px">Status</th>
              <th class="actions-col"></th>
            </tr>
          </thead>
          <tbody>
            @for (item of pagedItems(); track item.id) {
              <tr class="data-row" (click)="openView(item)">
                <td><span class="mono-text">{{ item.orderNumber }}</span></td>
                <td><span class="main-text">{{ item.customerName }}</span></td>
                <td><span class="muted-text">{{ getItemCount(item) }} items</span></td>
                <td>
                  <span class="badge" [style.background]="getPaymentBadge(item)[0]" [style.color]="getPaymentBadge(item)[1]">
                    {{ getPaymentLabel(item) }}
                  </span>
                </td>
                <td><span class="money-text">৳ {{ item.netAmount | number:'1.0-0' }}</span></td>
                <td><span [style.color]="getDueColor(item)">{{ getDue(item) }}</span></td>
                <td>
                  <span class="badge" [style.background]="getStatusBadge(item)[0]" [style.color]="getStatusBadge(item)[1]">
                    {{ getStatusLabel(item.status) }}
                  </span>
                </td>
                <td class="actions-col">
                  <button class="action-btn" (click)="openView(item); $event.stopPropagation()">→</button>
                  <button class="action-btn" (click)="openEdit(item); $event.stopPropagation()">✎</button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="8" class="empty-row">No sales orders found.</td></tr>
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
      [mode]="drawerMode()"
      [fields]="drawerFields"
      [entity]="currentEntity()"
      [saving]="saving()"
      (closeDrawer)="drawerOpen.set(false)"
      (saveEntity)="onSave($event)"
    />
  `,
  styles: [`
    :host { display: block; }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .page-title {
      font-size: 22px;
      font-weight: 800;
      letter-spacing: -0.01em;
      color: var(--text-primary);
      margin: 0;
    }

    .header-actions {
      display: flex;
      gap: 8px;
    }

    .btn-primary {
      padding: 9px 18px;
      border-radius: 7px;
      border: none;
      background: var(--primary);
      color: #fff;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      box-shadow: var(--shadow-btn);
    }
    .btn-primary:hover { background: var(--primary-hover); }

    .btn-secondary {
      padding: 9px 18px;
      border-radius: 7px;
      border: 1px solid var(--border-input);
      background: var(--surface);
      color: var(--text-secondary);
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
    }
    .btn-secondary:hover { background: var(--fill-subtle); }

    /* Pipeline */
    .pipeline-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 14px;
      margin-bottom: 20px;
    }

    .pipeline-card {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--radius-card);
      padding: 14px 16px;
      cursor: pointer;
      transition: all 0.15s;
    }
    .pipeline-card:hover {
      border-color: var(--primary-tint3);
    }
    .pipeline-card.active {
      background: var(--primary-tint1);
      border-color: var(--primary);
    }

    .pipe-header {
      display: flex;
      align-items: center;
      gap: 6px;
      margin-bottom: 6px;
    }

    .pipe-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }

    .pipe-label {
      font-size: 13px;
      font-weight: 600;
      color: var(--text-secondary);
    }

    .pipe-count {
      font-size: 22px;
      font-weight: 800;
      color: var(--text-primary);
      letter-spacing: -0.02em;
    }

    .pipe-sub {
      font-size: 12px;
      color: var(--text-muted);
    }

    /* Table */
    .table-card {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--radius-card);
      overflow: hidden;
    }

    .table-toolbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      border-bottom: 1px solid var(--border-row);
    }

    .search-box { width: 240px; }

    .search-input {
      width: 100%;
      padding: 8px 12px;
      border: 1px solid var(--border-input);
      border-radius: 7px;
      font-size: 13px;
      outline: none;
    }
    .search-input:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(234,88,12,0.12);
    }

    .result-count {
      font-size: 12px;
      color: var(--text-muted);
    }

    .table-scroll { overflow-x: auto; }

    .data-table {
      width: 100%;
      border-collapse: collapse;
      min-width: 940px;
    }

    .data-table th {
      padding: 10px 14px;
      text-align: left;
      font-size: 12px;
      font-weight: 700;
      color: var(--text-muted);
      text-transform: uppercase;
      letter-spacing: 0.04em;
      border-bottom: 1px solid var(--border);
      background: var(--fill-subtle);
      white-space: nowrap;
    }

    .data-table td {
      padding: 12px 14px;
      font-size: 13px;
      border-bottom: 1px solid var(--border-row);
    }

    .data-row { cursor: pointer; transition: background 0.1s; }
    .data-row:hover { background: var(--fill-subtle); }

    .mono-text { font-family: var(--font-mono); font-size: 12px; color: var(--text-muted); }
    .main-text { font-weight: 600; }
    .muted-text { font-size: 12px; color: var(--text-muted); }
    .money-text { font-weight: 700; }

    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: var(--radius-pill);
      font-size: 12px;
      font-weight: 600;
    }

    .actions-col { width: 70px; text-align: center; }

    .action-btn {
      width: 28px; height: 28px;
      border: 1px solid var(--border);
      border-radius: 5px;
      background: var(--surface);
      cursor: pointer;
      font-size: 13px;
      margin: 0 2px;
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }
    .action-btn:hover { background: var(--fill-subtle); }

    .empty-row { text-align: center; padding: 40px 14px !important; color: var(--text-muted); }

    .table-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      border-top: 1px solid var(--border-row);
    }

    .footer-info { font-size: 12px; color: var(--text-muted); }

    .pagination { display: flex; gap: 4px; }

    .page-btn {
      width: 30px; height: 30px;
      border: 1px solid var(--border);
      border-radius: 5px;
      background: var(--surface);
      cursor: pointer;
      font-size: 12px;
      font-weight: 600;
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--text-secondary);
    }
    .page-btn.active { background: var(--primary); color: #fff; border-color: var(--primary); }
    .page-btn:hover:not(.active):not(:disabled) { background: var(--fill-subtle); }
    .page-btn:disabled { opacity: 0.4; cursor: not-allowed; }
  `],
})
export class SalesOrderListComponent implements OnInit {
  private api = inject(ApiService);

  items = signal<SalesOrder[]>([]);
  query = signal('');
  activeStatus = signal('');
  currentPage = signal(1);
  pageSize = 15;
  drawerOpen = signal(false);
  drawerMode = signal<'view' | 'edit' | 'new'>('new');
  currentEntity = signal<any>(null);
  saving = signal(false);

  Math = Math;

  readonly statusBadgeMap: Record<number, [string, string]> = {
    0: ['#f4f5f7', '#6b7280'],
    1: ['#eff6ff', '#1d4ed8'],
    2: ['#fefce8', '#a16207'],
    3: ['#f0fdf4', '#15803d'],
    4: ['#fef2f2', '#dc2626'],
  };

  readonly paymentBadgeMap: Record<number, [string, string]> = {
    0: ['#f0fdf4', '#15803d'],
    1: ['#fefce8', '#a16207'],
    2: ['#eff6ff', '#1d4ed8'],
    3: ['#faf5ff', '#7e22ce'],
  };

  readonly drawerFields: DrawerField[] = [
    { key: 'customerName', label: 'Customer', type: 'text', required: true },
    { key: 'orderDate', label: 'Date', type: 'date', required: true },
    { key: 'netAmount', label: 'Amount', type: 'number', required: true },
    { key: 'dueDate', label: 'Due Date', type: 'date' },
    { key: 'status', label: 'Status', type: 'select', options: [
      { label: 'Draft', value: 0 }, { label: 'Confirmed', value: 1 },
      { label: 'Partially Delivered', value: 2 }, { label: 'Delivered', value: 3 },
    ]},
    { key: 'isCreditSale', label: 'Credit Sale', type: 'toggle' },
    { key: 'notes', label: 'Notes', type: 'textarea' },
  ];

  pipelines = computed(() => {
    const items = this.items();
    const draft = items.filter(i => i.status === 0).length;
    const confirmed = items.filter(i => i.status === 1).length;
    const partial = items.filter(i => i.status === 2).length;
    const delivered = items.filter(i => i.status === 3).length;
    const outstanding = items.filter(i => i.status === 1 || i.status === 2).reduce((s, i) => s + i.netAmount - (i.discount || 0), 0);
    return [
      { label: 'Draft', value: '0', count: draft, sub: 'awaiting confirmation', dotColor: '#6b7280' },
      { label: 'Confirmed', value: '1', count: confirmed, sub: 'ready to deliver', dotColor: '#1d4ed8' },
      { label: 'Partially paid', value: '2', count: partial, sub: '৳' + this.formatMoney(outstanding) + ' outstanding', dotColor: '#a16207' },
      { label: 'Delivered', value: '3', count: delivered, sub: 'this month', dotColor: '#15803d' },
    ];
  });

  filteredItems = computed(() => {
    let list = this.items();
    const q = this.query().toLowerCase();
    const status = this.activeStatus();

    if (status) {
      list = list.filter(i => String(i.status) === status);
    }

    if (q) {
      list = list.filter(i =>
        i.orderNumber?.toLowerCase().includes(q) ||
        i.customerName?.toLowerCase().includes(q)
      );
    }

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
    for (let i = Math.max(1, current - 2); i <= Math.min(total, current + 2); i++) {
      pages.push(i);
    }
    return pages;
  });

  drawerTitle = computed(() => {
    const mode = this.drawerMode();
    if (mode === 'new') return 'New Sales Order';
    if (mode === 'edit') return 'Edit Sales Order';
    return 'Sales Order';
  });

  ngOnInit() {
    this.api.getAll<SalesOrder>('salesorders').subscribe(data => this.items.set(data.items));
  }

  toggleStatus(value: string) {
    this.activeStatus.set(this.activeStatus() === value ? '' : value);
    this.currentPage.set(1);
  }

  getStatusLabel(status: number): string {
    const map: Record<number, string> = { 0: 'Draft', 1: 'Confirmed', 2: 'Partial', 3: 'Delivered', 4: 'Cancelled' };
    return map[status] ?? 'Unknown';
  }

  getStatusBadge(item: SalesOrder): [string, string] {
    return this.statusBadgeMap[item.status] || ['#f4f5f7', '#6b7280'];
  }

  getPaymentBadge(item: SalesOrder): [string, string] {
    const method = item.isCreditSale ? 1 : 0;
    return this.paymentBadgeMap[method] || ['#f4f5f7', '#6b7280'];
  }

  getPaymentLabel(item: SalesOrder): string {
    return item.isCreditSale ? 'Credit' : 'Cash';
  }

  getItemCount(item: SalesOrder): number {
    return item.items?.length || 0;
  }

  getDue(item: SalesOrder): string {
    const due = item.netAmount - (item.discount || 0);
    if (due <= 0) return '—';
    return `৳ ${due.toLocaleString('en-IN')}`;
  }

  getDueColor(item: SalesOrder): string {
    const due = item.netAmount - (item.discount || 0);
    return due > 0 ? '#a16207' : '#9ca3af';
  }

  openNew() {
    this.currentEntity.set({});
    this.drawerMode.set('new');
    this.drawerOpen.set(true);
  }

  openView(item: SalesOrder) {
    this.currentEntity.set({ ...item });
    this.drawerMode.set('view');
    this.drawerOpen.set(true);
  }

  openEdit(item: SalesOrder) {
    this.currentEntity.set({ ...item });
    this.drawerMode.set('edit');
    this.drawerOpen.set(true);
  }

  formatMoney(val: number): string {
    if (!val) return '0';
    if (val >= 100000) return (val / 100000).toFixed(2) + 'L';
    if (val >= 1000) return (val / 1000).toFixed(1) + 'K';
    return val.toLocaleString('en-IN');
  }

  onSave(data: any) {
    this.saving.set(true);
    const id = data.id || data.Id;
    const req$ = id
      ? this.api.update('salesorders', id, data)
      : this.api.create('salesorders', data);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.api.getAll<SalesOrder>('salesorders').subscribe(d => this.items.set(d.items));
      },
      error: () => this.saving.set(false),
    });
  }
}
