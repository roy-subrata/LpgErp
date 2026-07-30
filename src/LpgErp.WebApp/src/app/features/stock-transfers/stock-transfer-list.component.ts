import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { StockMovement } from '../../core/models';
import { StockTransferFormComponent } from './stock-transfer-form.component';

/**
 * A stock movement is a posted ledger row — the backend only ever exposes GET (history) and
 * POST (transfer), never PUT/DELETE, because editing or deleting one after the fact would
 * silently corrupt the stock levels it already adjusted. The generic EntityListComponent assumes
 * a single endpoint answers all four CRUD verbs, which doesn't hold here, so this screen is
 * hand-rolled: a read-only history table plus a dedicated "New Transfer" form.
 */
@Component({
  selector: 'app-stock-transfer-list',
  standalone: true,
  imports: [CommonModule, FormsModule, StockTransferFormComponent],
  template: `
    <div class="page-header">
      <h1 class="page-title">Stock Transfers</h1>
      <div class="header-actions">
        <button class="btn-primary-sm" (click)="formOpen.set(true)">+ New Transfer</button>
      </div>
    </div>

    <app-stock-transfer-form
      [open]="formOpen()"
      (close)="formOpen.set(false)"
      (saved)="onSaved()" />

    <div class="table-card">
      <div class="table-toolbar">
        <div class="search-box">
          <input type="text" class="search-input" placeholder="Search product, warehouse, reference..."
                 [ngModel]="query()" (ngModelChange)="query.set($event)" />
        </div>
        <span class="result-count">{{ filteredItems().length }} movements</span>
      </div>

      <div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width:22%">Product</th>
              <th style="width:14%">Type</th>
              <th style="width:16%">From</th>
              <th style="width:16%">To</th>
              <th style="width:10%">Quantity</th>
              <th style="width:14%">Reference</th>
              <th style="width:12%">Date</th>
            </tr>
          </thead>
          <tbody>
            @for (item of filteredItems(); track item.id) {
              <tr class="data-row">
                <td><span class="main-text">{{ item.productName }}</span></td>
                <td><span class="badge" [style.background]="typeBadge(item.type)[0]" [style.color]="typeBadge(item.type)[1]">{{ typeLabel(item.type) }}</span></td>
                <td><span class="muted-text">{{ item.fromWarehouseName || '—' }}</span></td>
                <td><span class="muted-text">{{ item.toWarehouseName || '—' }}</span></td>
                <td><span class="num-text">{{ item.quantity }}</span></td>
                <td><span class="mono-text">{{ item.reference || '—' }}</span></td>
                <td><span class="muted-text">{{ item.movementDate | date:'dd MMM yyyy' }}</span></td>
              </tr>
            } @empty {
              <tr><td colspan="7" class="empty-row">No stock movements found.</td></tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .header-actions { display: flex; gap: 8px; }
    .btn-primary-sm { padding: 9px 18px; border-radius: 7px; border: none; background: var(--primary); color: #fff; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: var(--shadow-btn); }
    .btn-primary-sm:hover { background: var(--primary-hover); }
    .table-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); overflow: hidden; }
    .table-toolbar { display: flex; justify-content: space-between; align-items: center; padding: 14px 18px; border-bottom: 1px solid var(--border); }
    .search-input { padding: 8px 12px; border: 1px solid var(--border-input); border-radius: 7px; font-size: 13px; width: 280px; }
    .result-count { font-size: 12px; color: var(--text-muted); }
    .table-scroll { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
    .data-table th { text-align: left; padding: 10px 16px; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em; color: var(--text-muted); border-bottom: 1px solid var(--border); }
    .data-row td { padding: 10px 16px; border-bottom: 1px solid var(--border-row); }
    .main-text { font-weight: 600; color: var(--text-primary); }
    .muted-text { color: var(--text-secondary); }
    .mono-text { font-family: 'JetBrains Mono', monospace; font-size: 12px; color: var(--text-secondary); }
    .num-text { font-variant-numeric: tabular-nums; }
    .badge { padding: 3px 9px; border-radius: 20px; font-size: 11px; font-weight: 600; }
    .empty-row { text-align: center; padding: 40px 14px !important; color: var(--text-muted); }
  `],
})
export class StockTransferListComponent implements OnInit {
  private api = inject(ApiService);

  items = signal<StockMovement[]>([]);
  query = signal('');
  formOpen = signal(false);

  filteredItems = computed(() => {
    const q = this.query().toLowerCase();
    if (!q) return this.items();
    return this.items().filter(i =>
      (i.productName ?? '').toLowerCase().includes(q) ||
      (i.fromWarehouseName ?? '').toLowerCase().includes(q) ||
      (i.toWarehouseName ?? '').toLowerCase().includes(q) ||
      (i.reference ?? '').toLowerCase().includes(q));
  });

  // Matches Domain.Entities.StockMovementType.
  private readonly typeLabels: Record<number, string> = {
    0: 'Purchase In', 1: 'Sale Out', 2: 'Transfer In', 3: 'Transfer Out',
    4: 'Adjustment', 5: 'Return', 6: 'Return to Company',
  };
  private readonly typeBadges: Record<number, string[]> = {
    0: ['#f0fdf4', '#15803d'], 1: ['#fefce8', '#a16207'],
    2: ['#eff6ff', '#1d4ed8'], 3: ['#eff6ff', '#1d4ed8'],
    4: ['#f4f5f7', '#6b7280'], 5: ['#faf5ff', '#7e22ce'],
    6: ['#fef2f2', '#b91c1c'],
  };

  typeLabel(type: number): string {
    return this.typeLabels[type] ?? 'Unknown';
  }

  typeBadge(type: number): string[] {
    return this.typeBadges[type] ?? ['#f4f5f7', '#6b7280'];
  }

  ngOnInit() {
    this.load();
  }

  load() {
    this.api.getAll<StockMovement>('stocktransfer/history', 1, 500).subscribe(data => this.items.set(data.items));
  }

  onSaved() {
    this.formOpen.set(false);
    this.load();
  }
}
