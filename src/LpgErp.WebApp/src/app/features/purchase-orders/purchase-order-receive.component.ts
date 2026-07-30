import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
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

@Component({
  selector: 'app-purchase-order-receive',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="'Receive Goods — ' + orderNumber()" (close)="onClose()">
      @if (lines().length === 0) {
        <p class="empty">This order has no items to receive.</p>
      } @else {
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
          <button type="button" class="btn btn-secondary" (click)="onClose()">Cancel</button>
          <button type="button" class="btn btn-primary" [disabled]="saving()" (click)="submit()">
            {{ saving() ? 'Receiving...' : 'Confirm Receipt' }}
          </button>
        </div>
      }
    </app-dialog>
  `,
  styles: [`
    .hint { font-size: 0.85rem; color: #6b7280; margin: 0 0 1rem; }
    .empty { color: #6b7280; }
    .table-wrap { overflow-x: auto; }
    .receive-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
    .receive-table th, .receive-table td { padding: 0.4rem 0.5rem; text-align: center; border-bottom: 1px solid #eee; }
    .receive-table th.left, .receive-table td.left { text-align: left; }
    .receive-table input { width: 68px; padding: 0.35rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .error { color: #dc3545; font-size: 0.85rem; margin-top: 0.75rem; }
    .na { color: #9ca3af; }
    .section { margin: 1.25rem 0 0.5rem; font-size: 0.9rem; color: #555; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-primary:disabled { opacity: 0.6; cursor: default; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class PurchaseOrderReceiveComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() received = new EventEmitter<void>();

  orderNumber = signal('');
  lines = signal<ReceiveLine[]>([]);
  leakages = signal<LeakageLine[]>([]);
  saving = signal(false);
  error = signal('');

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open && this.entityId) {
      this.error.set('');
      this.saving.set(false);
      this.api.getById<any>('purchaseorders', this.entityId).subscribe(po => {
        this.orderNumber.set(po.orderNumber ?? '');
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
      });
    }
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
    this.api.post(`purchaseorders/${this.entityId}/receive`, { items, leakages }).subscribe({
      next: () => {
        this.saving.set(false);
        this.received.emit();
      },
      error: (e) => {
        this.saving.set(false);
        this.error.set(e?.error?.message ?? 'Failed to record receipt.');
      },
    });
  }

  onClose() {
    this.close.emit();
  }
}
