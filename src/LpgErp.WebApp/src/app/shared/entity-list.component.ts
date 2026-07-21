import { Component, EventEmitter, Input, Output, signal, computed, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../core/api.service';
import { EntityDrawerComponent, DrawerField } from './entity-drawer.component';

export interface TableColumn {
  key: string;
  label: string;
  kind?: 'text' | 'mono' | 'main' | 'badge' | 'money' | 'num' | 'muted' | 'date';
  sub?: string;
  badgeMap?: Record<string, string[]>;
  width?: string;
}

export interface EntityConfig {
  endpoint: string;
  title: string;
  singular: string;
  cols: TableColumn[];
  fields: DrawerField[];
  badgeMaps?: Record<string, Record<string, string[]>>;
}

@Component({
  selector: 'app-entity-list',
  standalone: true,
  imports: [CommonModule, FormsModule, EntityDrawerComponent],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">{{ config.title }}</h1>
      </div>
      <div class="header-actions">
        <button class="btn-secondary-sm" (click)="onExport()">↓ Export</button>
        <button class="btn-primary-sm" (click)="openNew()">+ New {{ config.singular }}</button>
      </div>
    </div>

    <div class="table-card">
      <div class="table-toolbar">
        <div class="search-box">
          <input type="text" class="search-input" placeholder="Search..." [ngModel]="query()" (ngModelChange)="query.set($event)" />
        </div>
        <div class="toolbar-right">
          @if (tabs.length > 0) {
            <div class="tab-group">
              <button class="tab-btn" [class.active]="activeTab() === ''" (click)="activeTab.set('')">All</button>
              @for (tab of tabs; track tab.value) {
                <button class="tab-btn" [class.active]="activeTab() === tab.value" (click)="activeTab.set(tab.value)">{{ tab.label }}</button>
              }
            </div>
          }
          <span class="result-count">{{ filteredItems().length }} results</span>
        </div>
      </div>

      <div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              @for (col of config.cols; track col.key) {
                <th [style.width]="col.width || ''">{{ col.label }}</th>
              }
              <th class="actions-col"></th>
            </tr>
          </thead>
          <tbody>
            @for (item of pagedItems(); track item['id'] || $index) {
              <tr class="data-row" (click)="openView(item)">
                @for (col of config.cols; track col.key) {
                  <td>
                    @switch (col.kind) {
                      @case ('mono') {
                        <span class="mono-text">{{ item[col.key] }}</span>
                      }
                      @case ('main') {
                        <div class="main-cell">
                          <span class="main-text">{{ item[col.key] }}</span>
                          @if (col.sub && item[col.sub]) {
                            <span class="sub-text">{{ item[col.sub] }}</span>
                          }
                        </div>
                      }
                      @case ('badge') {
                        @let badgeMap = col.badgeMap || config.badgeMaps?.[col.key] || {};
                        @let badgeVal = toStr(item[col.key]);
                        @let badgeEntry = badgeMap[badgeVal] || badgeMap[toStr(getStatusLabel(item[col.key]))];
                        @if (badgeEntry) {
                          @if (badgeEntry.length > 2) {
                            <span class="badge" [style.background]="badgeEntry[1]" [style.color]="badgeEntry[2]">{{ badgeEntry[0] }}</span>
                          } @else {
                            <span class="badge" [style.background]="badgeEntry[0]" [style.color]="badgeEntry[1]">{{ getDisplayText(col, item) }}</span>
                          }
                        } @else {
                          <span class="badge" style="background:#f4f5f7;color:#6b7280">{{ getDisplayText(col, item) }}</span>
                        }
                      }
                      @case ('money') {
                        <span class="money-text">৳ {{ item[col.key] | number:'1.0-0' }}</span>
                      }
                      @case ('num') {
                        <span class="num-text">{{ item[col.key] }}</span>
                      }
                      @case ('date') {
                        <span class="muted-text">{{ item[col.key] | date:'mediumDate' }}</span>
                      }
                      @case ('muted') {
                        <span class="muted-text">{{ item[col.key] }}</span>
                      }
                      @default {
                        <span>{{ item[col.key] }}</span>
                      }
                    }
                  </td>
                }
                <td class="actions-col">
                  <button class="action-btn" title="View" (click)="openView(item); $event.stopPropagation()">→</button>
                  <button class="action-btn" title="Edit" (click)="openEdit(item); $event.stopPropagation()">✎</button>
                  <button class="action-btn danger" title="Delete" (click)="onDelete(item); $event.stopPropagation()">🗑</button>
                </td>
              </tr>
            } @empty {
              <tr>
                <td [attr.colspan]="config.cols.length + 1" class="empty-row">No {{ config.title.toLowerCase() }} found.</td>
              </tr>
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
      [fields]="config.fields"
      [entity]="currentEntity()"
      [saving]="saving()"
      (closeDrawer)="closeDrawer()"
      (saveEntity)="onSave($event)"
    />
  `,
  styles: [`
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
      align-items: center;
    }

    .btn-primary-sm {
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
    .btn-primary-sm:hover { background: var(--primary-hover); }

    .btn-secondary-sm {
      padding: 9px 18px;
      border-radius: 7px;
      border: 1px solid var(--border-input);
      background: var(--surface);
      color: var(--text-secondary);
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
    }
    .btn-secondary-sm:hover { background: var(--fill-subtle); }

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
      gap: 12px;
      flex-wrap: wrap;
    }

    .search-box {
      width: 240px;
    }

    .search-input {
      width: 100%;
      padding: 8px 12px;
      border: 1px solid var(--border-input);
      border-radius: 7px;
      font-size: 13px;
      outline: none;
      background: var(--surface);
    }
    .search-input:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(234,88,12,0.12);
    }

    .toolbar-right {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .tab-group {
      display: flex;
      gap: 4px;
      background: var(--fill-subtle);
      border-radius: 7px;
      padding: 3px;
    }

    .tab-btn {
      padding: 5px 12px;
      border: none;
      border-radius: 5px;
      background: transparent;
      font-size: 12px;
      font-weight: 600;
      color: var(--text-muted);
      cursor: pointer;
      transition: all 0.15s;
    }
    .tab-btn.active {
      background: var(--surface);
      color: var(--text-primary);
      box-shadow: 0 1px 2px rgba(0,0,0,0.06);
    }
    .tab-btn:hover:not(.active) {
      color: var(--text-secondary);
    }

    .result-count {
      font-size: 12px;
      color: var(--text-muted);
    }

    .table-scroll {
      overflow-x: auto;
    }

    .data-table {
      width: 100%;
      border-collapse: collapse;
      min-width: 600px;
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
      color: var(--text-primary);
    }

    .data-row {
      cursor: pointer;
      transition: background 0.1s;
    }
    .data-row:hover {
      background: var(--fill-subtle);
    }

    .mono-text {
      font-family: var(--font-mono);
      font-size: 12px;
      color: var(--text-muted);
    }

    .main-cell {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }
    .main-text {
      font-weight: 600;
      color: var(--text-primary);
    }
    .sub-text {
      font-size: 12px;
      color: var(--text-muted);
    }

    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: var(--radius-pill);
      font-size: 12px;
      font-weight: 600;
      white-space: nowrap;
    }

    .money-text {
      font-weight: 700;
      color: var(--text-primary);
    }

    .num-text {
      font-weight: 600;
    }

    .muted-text {
      font-size: 12px;
      color: var(--text-muted);
    }

    .actions-col {
      width: 80px;
      text-align: center;
    }

    .action-btn {
      width: 28px;
      height: 28px;
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
    .action-btn.danger { color: var(--red-fg); border-color: var(--red-bg); }
    .action-btn.danger:hover { background: var(--red-bg); }

    .empty-row {
      text-align: center;
      padding: 40px 14px !important;
      color: var(--text-muted);
    }

    .table-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      border-top: 1px solid var(--border-row);
    }

    .footer-info {
      font-size: 12px;
      color: var(--text-muted);
    }

    .pagination {
      display: flex;
      gap: 4px;
    }

    .page-btn {
      width: 30px;
      height: 30px;
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
    .page-btn.active {
      background: var(--primary);
      color: #fff;
      border-color: var(--primary);
    }
    .page-btn:hover:not(.active):not(:disabled) { background: var(--fill-subtle); }
    .page-btn:disabled { opacity: 0.4; cursor: not-allowed; }
  `],
})
export class EntityListComponent implements OnInit {
  @Input() config!: EntityConfig;
  @Input() tabs: { label: string; value: string }[] = [];
  @Input() tabField?: string;
  @Input() searchFields?: string[];
  @Input() pageSize = 15;

  @Output() rowClicked = new EventEmitter<any>();

  private api = inject(ApiService);

  items = signal<any[]>([]);
  query = signal('');
  activeTab = signal('');
  currentPage = signal(1);
  drawerOpen = signal(false);
  drawerMode = signal<'view' | 'edit' | 'new'>('new');
  currentEntity = signal<any>(null);
  saving = signal(false);

  Math = Math;

  filteredItems = computed(() => {
    let list = this.items();
    const q = this.query().toLowerCase();
    const tab = this.activeTab();
    const tf = this.tabField;

    if (tab && tf) {
      list = list.filter(i => this.toStr(i[tf]) === tab);
    }

    if (q) {
      const fields = this.searchFields || this.config.cols.map(c => c.key);
      list = list.filter(i =>
        fields.some(f => this.toStr(i[f] ?? '').toLowerCase().includes(q))
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
    if (mode === 'new') return `New ${this.config.singular}`;
    if (mode === 'edit') return `Edit ${this.config.singular}`;
    return this.config.singular;
  });

  drawerSubtitle = computed(() => {
    const entity = this.currentEntity();
    if (entity?.id) return entity.id.substring(0, 8);
    return '';
  });

  ngOnInit() {
    this.load();
  }

  load() {
    this.api.getAllList<any>(this.config.endpoint).subscribe(data => {
      this.items.set(data);
    });
  }

  getDisplayText(col: TableColumn, item: any): string {
    const val = item[col.key];
    if (col.kind === 'badge' && col.badgeMap) {
      const statusLabel = this.getStatusLabel(val);
      if (col.badgeMap[statusLabel]) return statusLabel;
      if (col.badgeMap[this.toStr(val)]) return this.toStr(val);
    }
    return val ?? '';
  }

  toStr(val: any): string {
    if (val === null || val === undefined) return '';
    return String(val);
  }

  getStatusLabel(val: any): string {
    const map: Record<number, string> = {
      0: 'Draft', 1: 'Confirmed', 2: 'Partial', 3: 'Delivered', 4: 'Cancelled',
      5: 'Active', 6: 'Inactive', 7: 'Loading', 8: 'Selling', 9: 'Closed',
    };
    if (typeof val === 'number') return map[val] ?? this.toStr(val);
    return this.toStr(val);
  }

  openNew() {
    this.currentEntity.set({});
    this.drawerMode.set('new');
    this.drawerOpen.set(true);
  }

  openView(item: any) {
    this.currentEntity.set({ ...item });
    this.drawerMode.set('view');
    this.drawerOpen.set(true);
  }

  openEdit(item: any) {
    this.currentEntity.set({ ...item });
    this.drawerMode.set('edit');
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    this.drawerOpen.set(false);
  }

  onSave(data: any) {
    this.saving.set(true);
    const id = data.id || data.Id;
    const req$ = id
      ? this.api.update(this.config.endpoint, id, data)
      : this.api.create(this.config.endpoint, data);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeDrawer();
        this.load();
      },
      error: () => this.saving.set(false),
    });
  }

  onDelete(item: any) {
    if (confirm(`Delete this ${this.config.singular.toLowerCase()}?`)) {
      this.api.delete(this.config.endpoint, item.id || item.Id).subscribe(() => this.load());
    }
  }

  onExport() {
    // TODO: implement CSV/PDF export
  }
}
