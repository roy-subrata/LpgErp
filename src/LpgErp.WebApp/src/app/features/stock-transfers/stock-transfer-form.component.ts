import { Component, EventEmitter, Input, Output, inject, signal, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Product, Warehouse } from '../../core/models';

@Component({
  selector: 'app-stock-transfer-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" title="New Stock Transfer" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label>Product</label>
          <select [(ngModel)]="productId" name="productId" required>
            <option value="">Select Product</option>
            @for (p of products(); track p.id) {
              <option [value]="p.id">{{ p.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>From Warehouse</label>
          <select [(ngModel)]="fromWarehouseId" name="fromWarehouseId" required>
            <option value="">Select Source</option>
            @for (w of warehouses(); track w.id) {
              <option [value]="w.id">{{ w.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>To Warehouse</label>
          <select [(ngModel)]="toWarehouseId" name="toWarehouseId" required>
            <option value="">Select Destination</option>
            @for (w of warehouses(); track w.id) {
              <option [value]="w.id">{{ w.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>Quantity</label>
          <input type="number" [(ngModel)]="quantity" name="quantity" min="1" required />
        </div>
        <div class="form-group">
          <label>Reference</label>
          <input type="text" [(ngModel)]="reference" name="reference" />
        </div>
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="onClose()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">Transfer</button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group input, .form-group select { width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class StockTransferFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  products = signal<Product[]>([]);
  warehouses = signal<Warehouse[]>([]);
  productId = '';
  fromWarehouseId = '';
  toWarehouseId = '';
  quantity = 1;
  reference = '';
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.resetForm();
      this.api.getAllList<Product>('products').subscribe(p => this.products.set(p));
      this.api.getAllList<Warehouse>('warehouses').subscribe(w => this.warehouses.set(w));
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      productId: this.productId,
      fromWarehouseId: this.fromWarehouseId,
      toWarehouseId: this.toWarehouseId,
      quantity: this.quantity,
      reference: this.reference || null,
    };

    this.api.post('stocktransfer', body).subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.emit();
        this.resetForm();
      },
      error: () => this.saving.set(false),
    });
  }

  onClose() {
    this.resetForm();
    this.close.emit();
  }

  private resetForm() {
    this.productId = '';
    this.fromWarehouseId = '';
    this.toWarehouseId = '';
    this.quantity = 1;
    this.reference = '';
  }
}
