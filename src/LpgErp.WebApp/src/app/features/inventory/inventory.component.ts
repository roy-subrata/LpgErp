import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';

interface StockRow {
  productName: string;
  warehouseName: string;
  quantity: number;
  brandName: string;
  productType: string;
}

interface MovementRow {
  id: string;
  productName: string;
  type: number;
  quantity: number;
  fromWarehouseName: string | null;
  toWarehouseName: string | null;
  reference: string | null;
  movementDate: string;
}

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Inventory</h1>
        <p class="page-subtitle">Warehouse stock levels and movement ledger</p>
      </div>
    </div>

    <div class="tab-bar">
      <button class="tab" [class.active]="tab() === 'levels'" (click)="tab.set('levels')">Stock Levels</button>
      <button class="tab" [class.active]="tab() === 'movements'" (click)="tab.set('movements')">Movements</button>
    </div>

    @if (tab() === 'levels') {
      <div class="table-card">
        <div class="toolbar">
          <select class="filter-select" [ngModel]="warehouseFilter()" (ngModelChange)="onWarehouseChange($event)">
            <option value="">All warehouses</option>
            @for (w of warehouses(); track w.id) {
              <option [value]="w.id">{{ w.name }}</option>
            }
          </select>
          <input type="text" class="search-input" placeholder="Search product / brand..." [ngModel]="query()" (ngModelChange)="query.set($event)" />
          <span class="result-count">{{ filteredLevels().length }} items · {{ totalUnits() }} units</span>
        </div>
        <div class="table-wrap">
          <table>
            <thead>
              <tr><th>Product</th><th>Brand</th><th>Type</th><th>Warehouse</th><th class="num">Quantity</th></tr>
            </thead>
            <tbody>
              @for (row of filteredLevels(); track row.productName + row.warehouseName) {
                <tr>
                  <td class="strong">{{ row.productName }}</td>
                  <td>{{ row.brandName || '—' }}</td>
                  <td><span class="type-chip">{{ row.productType }}</span></td>
                  <td>{{ row.warehouseName }}</td>
                  <td class="num" [class.low]="row.quantity <= 10">{{ row.quantity }}</td>
                </tr>
              } @empty {
                <tr><td colspan="5" class="empty-cell">No stock recorded. Stock enters via purchase receiving.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    } @else {
      <div class="table-card">
        <div class="table-wrap">
          <table>
            <thead>
              <tr><th>Date</th><th>Product</th><th>Type</th><th class="num">Qty</th><th>From</th><th>To</th><th>Reference</th></tr>
            </thead>
            <tbody>
              @for (m of movements(); track m.id) {
                <tr>
                  <td>{{ m.movementDate | date:'d MMM y, h:mm a' }}</td>
                  <td class="strong">{{ m.productName }}</td>
                  <td><span class="type-chip" [ngClass]="'mv-' + m.type">{{ movementType(m.type) }}</span></td>
                  <td class="num">{{ m.quantity }}</td>
                  <td>{{ m.fromWarehouseName || '—' }}</td>
                  <td>{{ m.toWarehouseName || '—' }}</td>
                  <td class="mono">{{ m.reference || '—' }}</td>
                </tr>
              } @empty {
                <tr><td colspan="7" class="empty-cell">No stock movements yet.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }
    .page-header { margin-bottom: 16px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .page-subtitle { font-size: 13px; color: var(--text-muted); margin: 4px 0 0; }
    .tab-bar { display: inline-flex; gap: 4px; background: var(--fill-subtle); border: 1px solid var(--border); border-radius: var(--radius-pill); padding: 3px; margin-bottom: 16px; }
    .tab { padding: 6px 18px; border: none; border-radius: var(--radius-pill); background: transparent; font-size: 13px; font-weight: 600; color: var(--text-muted); cursor: pointer; }
    .tab.active { background: var(--surface); color: var(--text-primary); box-shadow: 0 1px 2px rgba(0,0,0,0.06); }
    .table-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); overflow: hidden; }
    .toolbar { display: flex; align-items: center; gap: 12px; padding: 12px 16px; border-bottom: 1px solid var(--border-row); }
    .filter-select { padding: 7px 10px; border: 1px solid var(--border-input); border-radius: var(--radius-btn); font-size: 13px; background: var(--surface); color: var(--text-primary); }
    .search-input { width: 220px; padding: 7px 12px; border: 1px solid var(--border-input); border-radius: var(--radius-btn); font-size: 13px; background: var(--surface); color: var(--text-primary); outline: none; }
    .result-count { margin-left: auto; font-size: 12px; color: var(--text-faint); }
    .table-wrap { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; }
    thead tr { background: var(--fill-subtle); }
    th { padding: 9px 14px; text-align: left; font-size: 11px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.04em; border-bottom: 1px solid var(--border); }
    td { padding: 10px 14px; font-size: 13px; color: var(--text-secondary); border-bottom: 1px solid var(--border-row); }
    .strong { font-weight: 600; color: var(--text-primary); }
    .num { text-align: right; font-weight: 700; font-family: var(--font-mono); }
    .num.low { color: #dc2626; }
    .mono { font-family: var(--font-mono); font-size: 12px; }
    .type-chip { display: inline-block; padding: 2px 8px; border-radius: 10px; font-size: 11px; font-weight: 600; background: var(--fill-subtle); color: var(--text-secondary); }
    .mv-0 { background: #f0fdf4; color: #15803d; }
    .mv-1 { background: #fef2f2; color: #dc2626; }
    .mv-2, .mv-3 { background: #eff6ff; color: #1d4ed8; }
    .mv-4 { background: #fefce8; color: #a16207; }
    .mv-5 { background: #f5f3ff; color: #6d28d9; }
    .empty-cell { text-align: center; color: var(--text-muted); padding: 40px 14px !important; }
  `],
})
export class InventoryComponent implements OnInit {
  private api = inject(ApiService);

  tab = signal<'levels' | 'movements'>('levels');
  levels = signal<StockRow[]>([]);
  movements = signal<MovementRow[]>([]);
  warehouses = signal<any[]>([]);
  warehouseFilter = signal('');
  query = signal('');

  filteredLevels = computed(() => {
    const q = this.query().toLowerCase();
    return this.levels().filter(r =>
      !q || r.productName.toLowerCase().includes(q) || (r.brandName || '').toLowerCase().includes(q));
  });

  totalUnits = computed(() => this.filteredLevels().reduce((s, r) => s + r.quantity, 0));

  ngOnInit() {
    this.api.getAllList<any>('warehouses').subscribe(d => this.warehouses.set(d));
    this.loadLevels();
    this.api.getAll<MovementRow>('stocktransfer/history', 1, 100).subscribe(d => this.movements.set(d.items));
  }

  onWarehouseChange(id: string) {
    this.warehouseFilter.set(id);
    this.loadLevels();
  }

  private loadLevels() {
    const params = this.warehouseFilter() ? { warehouseId: this.warehouseFilter() } : undefined;
    this.api.get<StockRow[]>('reports/inventory', params).subscribe(d => this.levels.set(d ?? []));
  }

  movementType(type: number): string {
    const map: Record<number, string> = {
      0: 'Purchase In', 1: 'Sale Out', 2: 'Transfer In', 3: 'Transfer Out', 4: 'Adjustment', 5: 'Return',
    };
    return map[type] ?? String(type);
  }
}
