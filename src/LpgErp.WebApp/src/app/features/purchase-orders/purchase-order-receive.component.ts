import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';

interface ReceiveLine {
  productId: string;
  productName: string;
  orderedQuantity: number;
  alreadyReceived: number;
  receivedQuantity: number;
  damagedQuantity: number;
  missingQuantity: number;
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
                </tr>
              }
            </tbody>
          </table>
        </div>
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
          orderedQuantity: i.orderedQuantity ?? 0,
          alreadyReceived: i.receivedQuantity ?? 0,
          receivedQuantity: 0,
          damagedQuantity: 0,
          missingQuantity: 0,
        })));
      });
    }
  }

  outstanding(line: ReceiveLine): number {
    return Math.max(0, line.orderedQuantity - line.alreadyReceived);
  }

  submit() {
    const items = this.lines()
      .filter(l => l.receivedQuantity > 0 || l.damagedQuantity > 0 || l.missingQuantity > 0)
      .map(l => ({
        productId: l.productId,
        receivedQuantity: l.receivedQuantity || 0,
        damagedQuantity: l.damagedQuantity || 0,
        missingQuantity: l.missingQuantity || 0,
      }));

    if (items.length === 0) {
      this.error.set('Enter at least one received, damaged, or missing quantity.');
      return;
    }
    if (items.some(i => i.damagedQuantity > i.receivedQuantity)) {
      this.error.set('Damaged units cannot exceed the quantity received on the same line.');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.api.post(`purchaseorders/${this.entityId}/receive`, { items }).subscribe({
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
