import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../core/api.service';

interface ReceivablePurchaseOrder {
  id: string;
  orderNumber: string;
  supplierName: string;
  warehouseName: string;
  orderDate: string;
  expectedDeliveryDate: string | null;
  totalAmount: number;
  netPayable: number;
  status: number;
}

type ReceiptTab = 'awaiting' | 'received';

/**
 * Goods Receipt — its own feature, separate from the Purchase Orders list, the same way
 * delivering a sale is separate from placing it. Previously "Receive goods" was a dialog opened
 * from a row menu, layered on top of a list with its own dropdown still mounted; that combination
 * would hang the page on click. A dedicated page sidesteps that interaction entirely, and gives
 * receiving its own clear home instead of being hidden inside a ⋮ menu.
 */
@Component({
  selector: 'app-goods-receipt-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Goods Receipt</h1>
        <span class="page-sub">Purchase orders awaiting receipt, and what's already come in</span>
      </div>
    </div>

    <div class="table-card">
      <div class="table-toolbar">
        <div class="tab-group">
          <button class="tab-btn" [class.active]="tab() === 'awaiting'" (click)="tab.set('awaiting')">Awaiting Receipt</button>
          <button class="tab-btn" [class.active]="tab() === 'received'" (click)="tab.set('received')">Received</button>
        </div>
        <div class="search-box">
          <input type="text" class="search-input" placeholder="Search order, supplier..."
                 [ngModel]="query()" (ngModelChange)="query.set($event)" />
        </div>
        <span class="result-count">{{ filteredItems().length }} {{ tab() === 'awaiting' ? 'awaiting receipt' : 'received' }}</span>
      </div>

      <div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width:16%">Order</th>
              <th style="width:20%">Supplier</th>
              <th style="width:14%">Warehouse</th>
              <th style="width:12%">Order Date</th>
              <th style="width:12%">Expected</th>
              <th style="width:12%" class="right">Net Payable</th>
              <th style="width:10%">Status</th>
              <th class="actions-col"></th>
            </tr>
          </thead>
          <tbody>
            @for (po of filteredItems(); track po.id) {
              <tr class="data-row" (click)="openReceive(po)">
                <td><span class="mono-text">{{ po.orderNumber }}</span></td>
                <td><span class="main-text">{{ po.supplierName }}</span></td>
                <td><span class="muted-text">{{ po.warehouseName }}</span></td>
                <td><span class="muted-text">{{ po.orderDate | date:'dd MMM yyyy' }}</span></td>
                <td><span class="muted-text">{{ po.expectedDeliveryDate ? (po.expectedDeliveryDate | date:'dd MMM yyyy') : '—' }}</span></td>
                <td class="right"><span class="money-text">৳ {{ po.netPayable | number:'1.0-0' }}</span></td>
                <td>
                  <span class="badge" [style.background]="statusBadge[po.status]?.[1]" [style.color]="statusBadge[po.status]?.[2]">
                    {{ statusBadge[po.status]?.[0] }}
                  </span>
                </td>
                <td class="actions-col">
                  <button class="btn-receive" (click)="openReceive(po); $event.stopPropagation()">
                    {{ po.status === 4 ? 'View →' : 'Receive →' }}
                  </button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="8" class="empty-row">
                {{ tab() === 'awaiting' ? 'Nothing waiting to be received — every confirmed order is fully in stock.' : 'No fully received orders yet.' }}
              </td></tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .page-sub { font-size: 12px; color: var(--text-muted); }
    .table-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); overflow: hidden; }
    .table-toolbar { display: flex; justify-content: space-between; align-items: center; padding: 14px 18px; border-bottom: 1px solid var(--border); gap: 12px; flex-wrap: wrap; }
    .tab-group { display: flex; gap: 4px; background: var(--fill-subtle); border-radius: 7px; padding: 3px; }
    .tab-btn { padding: 5px 12px; border: none; border-radius: 5px; background: transparent; font-size: 12px; font-weight: 600; color: var(--text-muted); cursor: pointer; transition: all 0.15s; }
    .tab-btn.active { background: var(--surface); color: var(--text-primary); box-shadow: 0 1px 2px rgba(0,0,0,0.06); }
    .tab-btn:hover:not(.active) { color: var(--text-secondary); }
    .search-input { padding: 8px 12px; border: 1px solid var(--border-input); border-radius: 7px; font-size: 13px; width: 280px; }
    .result-count { font-size: 12px; color: var(--text-muted); }
    .table-scroll { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
    .data-table th { text-align: left; padding: 10px 16px; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em; color: var(--text-muted); border-bottom: 1px solid var(--border); }
    .data-row { cursor: pointer; }
    .data-row td { padding: 10px 16px; border-bottom: 1px solid var(--border-row); }
    .data-row:hover { background: var(--fill-subtle); }
    .main-text { font-weight: 600; color: var(--text-primary); }
    .muted-text { color: var(--text-secondary); }
    .mono-text { font-family: 'JetBrains Mono', monospace; font-size: 12px; }
    .right { text-align: right; }
    .badge { display: inline-block; padding: 3px 9px; border-radius: 20px; font-size: 11px; font-weight: 600; }
    .actions-col { width: 110px; text-align: right; }
    .btn-receive { padding: 6px 12px; border-radius: 6px; border: 1px solid var(--primary, #ea580c); background: none; color: var(--primary, #ea580c); font-size: 12px; font-weight: 600; cursor: pointer; }
    .btn-receive:hover { background: var(--primary, #ea580c); color: #fff; }
    .empty-row { text-align: center; padding: 40px 14px !important; color: var(--text-muted); }
  `],
})
export class GoodsReceiptListComponent implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);

  items = signal<ReceivablePurchaseOrder[]>([]);
  query = signal('');
  tab = signal<ReceiptTab>('awaiting');

  // Matches PurchaseOrderStatus.
  readonly statusBadge: Record<number, string[]> = {
    1: ['Confirmed', '#dbeafe', '#1d4ed8'],
    2: ['In Transit', '#e0e7ff', '#4338ca'],
    3: ['Partially Received', '#fef3c7', '#92400e'],
    4: ['Received', '#dcfce7', '#166534'],
  };

  filteredItems = computed(() => {
    const statuses = this.tab() === 'awaiting' ? [1, 2, 3] : [4];
    let list = this.items().filter(po => statuses.includes(po.status));
    const q = this.query().toLowerCase();
    if (q) {
      list = list.filter(po =>
        po.orderNumber.toLowerCase().includes(q) ||
        po.supplierName.toLowerCase().includes(q));
    }
    return list;
  });

  ngOnInit() {
    this.load();
  }

  load() {
    this.api.getAll<ReceivablePurchaseOrder>('purchaseorders', 1, 200).subscribe(data => {
      // Confirmed, InTransit, PartiallyReceived (awaiting) plus Received (viewable) — Draft hasn't
      // been confirmed yet and Cancelled never will be, so neither belongs here.
      this.items.set(data.items.filter(po => [1, 2, 3, 4].includes(po.status)));
    });
  }

  openReceive(po: ReceivablePurchaseOrder) {
    this.router.navigate(['/goods-receipt', po.id]);
  }
}
