import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ApiService } from '../../core/api.service';

interface ReceiveLine {
  productId: string;
  productName: string;
  productType: number;
  orderedQuantity: number;
  alreadyReceived: number;
  receivedQuantity: number;
  damagedQuantity: number;
  missingQuantity: number;
  /** Empties going back to the company for this refill line. Blank = one per refill received. */
  emptySentQuantity: number | null;
  emptyAlreadySent: number;
}

interface LeakageLine {
  id: string;
  label: string;
  quantity: number;
  alreadySettled: number;
  resolution: number;
  creditAmount: number;
  settledQuantity: number;
}

/**
 * The receiving side of a purchase order, on its own page — how many of each item actually
 * arrived, whether any are damaged or short, how many empty cylinders went back for refills, and
 * how any leaking cylinders on this order were settled. Posts to the same endpoint the old
 * receive dialog used; only the presentation changed, from a modal to a full page.
 */
@Component({
  selector: 'app-goods-receipt-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="page-header">
      <div>
        <a routerLink="/goods-receipt" class="back-link">← Goods Receipt</a>
        <h1 class="page-title">Receive — {{ orderNumber() }}</h1>
        <span class="page-sub">{{ supplierName() }}{{ warehouseName() ? ' · ' + warehouseName() : '' }}</span>
      </div>
    </div>

    @if (loading()) {
      <div class="state-note">Loading…</div>
    } @else if (notFound()) {
      <div class="state-note error">This order could not be found, or has already been fully received.</div>
    } @else {
      <div class="form-card">
        <p class="hint">Enter the quantities physically received, plus any damaged or missing (short) units. Only good (received − damaged) units are added to warehouse stock.</p>
        <p class="hint">Leave <strong>Empties Out</strong> blank to send one empty cylinder per refill received — the normal swap. Enter a smaller number if you sent fewer; the rest stays owed to the company.</p>

        <div class="table-wrap">
          <table class="receive-table">
            <thead>
              <tr>
                <th class="left">Product</th>
                <th>Ordered</th>
                <th>Recv'd</th>
                <th>Outstanding</th>
                <th>Receive</th>
                <th>Damaged</th>
                <th>Missing</th>
                <th title="Empty cylinders handed back to the company">Empties Out</th>
              </tr>
            </thead>
            <tbody>
              @for (line of lines(); track line.productId) {
                <tr>
                  <td class="left">{{ line.productName }}</td>
                  <td>{{ line.orderedQuantity }}</td>
                  <td>{{ line.alreadyReceived }}</td>
                  <td>{{ outstanding(line) }}</td>
                  <td><input type="number" min="0" [(ngModel)]="line.receivedQuantity" [name]="'recv_' + line.productId" /></td>
                  <td><input type="number" min="0" [(ngModel)]="line.damagedQuantity" [name]="'dmg_' + line.productId" /></td>
                  <td><input type="number" min="0" [(ngModel)]="line.missingQuantity" [name]="'miss_' + line.productId" /></td>
                  <td>
                    @if (line.productType === 1) {
                      <input type="number" min="0" [placeholder]="line.receivedQuantity || '='"
                             [(ngModel)]="line.emptySentQuantity" [name]="'empty_' + line.productId" />
                    } @else {
                      <span class="na">—</span>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        @if (leakages().length > 0) {
          <h4 class="section">Leaking cylinders returned</h4>
          <div class="table-wrap">
            <table class="receive-table">
              <thead>
                <tr>
                  <th class="left">Cylinder</th>
                  <th>Returned</th>
                  <th>Settled</th>
                  <th class="left">Company gives</th>
                  <th>Settle now</th>
                </tr>
              </thead>
              <tbody>
                @for (leak of leakages(); track leak.id) {
                  <tr>
                    <td class="left">{{ leak.label }}</td>
                    <td>{{ leak.quantity }}</td>
                    <td>{{ leak.alreadySettled }}</td>
                    <td class="left">{{ resolutionLabel(leak) }}</td>
                    <td><input type="number" min="0" [(ngModel)]="leak.settledQuantity" [name]="'leak_' + leak.id" /></td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }

        @if (error()) { <p class="error">{{ error() }}</p> }

        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="cancel()">Cancel</button>
          <button type="button" class="btn btn-primary" [disabled]="saving()" (click)="submit()">
            {{ saving() ? 'Receiving...' : 'Confirm Receipt' }}
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-header { margin-bottom: 20px; }
    .back-link { font-size: 12px; color: var(--text-muted, #6b7280); text-decoration: none; }
    .back-link:hover { text-decoration: underline; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 4px 0 0; }
    .page-sub { font-size: 12px; color: var(--text-muted); }
    .state-note { padding: 24px; color: #6b7280; }
    .state-note.error { color: #b91c1c; }

    .form-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 20px; }
    .hint { font-size: 0.85rem; color: #6b7280; margin: 0 0 1rem; }
    .table-wrap { overflow-x: auto; }
    .receive-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
    .receive-table th, .receive-table td { padding: 0.5rem 0.6rem; text-align: center; border-bottom: 1px solid #eee; }
    .receive-table th.left, .receive-table td.left { text-align: left; }
    .receive-table input { width: 74px; padding: 0.4rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .na { color: #9ca3af; }
    .section { margin: 1.5rem 0 0.5rem; font-size: 0.95rem; color: #555; }
    .error { color: #dc3545; font-size: 0.85rem; margin-top: 0.75rem; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.55rem 1.1rem; border-radius: 6px; cursor: pointer; border: 1px solid #ddd; font-size: 0.9rem; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-primary:disabled { opacity: 0.6; cursor: default; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class GoodsReceiptFormComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  orderId = '';
  orderNumber = signal('');
  supplierName = signal('');
  warehouseName = signal('');
  lines = signal<ReceiveLine[]>([]);
  leakages = signal<LeakageLine[]>([]);
  loading = signal(true);
  notFound = signal(false);
  saving = signal(false);
  error = signal('');

  ngOnInit() {
    this.orderId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.orderId) {
      this.notFound.set(true);
      this.loading.set(false);
      return;
    }

    this.api.getById<any>('purchaseorders', this.orderId).subscribe({
      next: po => {
        this.orderNumber.set(po.orderNumber ?? '');
        this.supplierName.set(po.supplierName ?? '');
        this.warehouseName.set(po.warehouseName ?? '');
        this.lines.set((po.items ?? []).map((i: any) => ({
          productId: i.productId,
          productName: i.productName ?? i.productId,
          productType: i.productType ?? 0,
          orderedQuantity: i.orderedQuantity ?? 0,
          alreadyReceived: i.receivedQuantity ?? 0,
          receivedQuantity: 0,
          damagedQuantity: 0,
          missingQuantity: 0,
          emptySentQuantity: null,
          emptyAlreadySent: i.emptySentQuantity ?? 0,
        })));
        this.leakages.set((po.leakages ?? []).map((l: any) => ({
          id: l.id,
          label: `${l.brandName ?? ''} ${l.cylinderSizeName ?? ''}`.trim(),
          quantity: l.quantity ?? 0,
          alreadySettled: l.settledQuantity ?? 0,
          resolution: l.resolution ?? 0,
          creditAmount: l.creditAmount ?? 0,
          settledQuantity: 0,
        })));
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  outstanding(line: ReceiveLine): number {
    return Math.max(0, line.orderedQuantity - line.alreadyReceived);
  }

  resolutionLabel(leak: LeakageLine): string {
    switch (leak.resolution) {
      case 0: return 'Free refill';
      case 1: return `Credit ৳${leak.creditAmount}`;
      default: return 'Replacement cylinder';
    }
  }

  submit() {
    const items = this.lines()
      .filter(l => l.receivedQuantity > 0 || l.damagedQuantity > 0 || l.missingQuantity > 0)
      .map(l => ({
        productId: l.productId,
        receivedQuantity: l.receivedQuantity || 0,
        damagedQuantity: l.damagedQuantity || 0,
        missingQuantity: l.missingQuantity || 0,
        // Blank means the normal one-for-one swap; the server works it out from what arrived.
        emptySentQuantity: l.emptySentQuantity === null || (l.emptySentQuantity as any) === ''
          ? null
          : Number(l.emptySentQuantity),
      }));

    const leakages = this.leakages()
      .filter(l => l.settledQuantity > 0)
      .map(l => ({ leakageId: l.id, settledQuantity: Number(l.settledQuantity) }));

    if (items.length === 0 && this.leakages().every(l => l.settledQuantity <= 0)) {
      this.error.set('Enter at least one received, damaged, missing, or settled quantity.');
      return;
    }
    if (items.some(i => i.damagedQuantity > i.receivedQuantity)) {
      this.error.set('Damaged units cannot exceed the quantity received on the same line.');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.api.post(`purchaseorders/${this.orderId}/receive`, { items, leakages }).subscribe({
      next: () => {
        this.saving.set(false);
        this.router.navigate(['/goods-receipt']);
      },
      error: (e) => {
        this.saving.set(false);
        this.error.set(e?.error?.errors?.[0] ?? e?.error?.message ?? 'Failed to record receipt.');
      },
    });
  }

  cancel() {
    this.router.navigate(['/goods-receipt']);
  }
}
