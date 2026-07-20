import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Supplier, Warehouse, Product, TransportCompany } from '../../core/models';

interface OrderItem {
  productId: string;
  orderedQuantity: number;
  unitPrice: number;
}

@Component({
  selector: 'app-purchase-order-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Purchase Order' : 'New Purchase Order'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="supplierId">Supplier</label>
          <select id="supplierId" [(ngModel)]="supplierId" name="supplierId" required>
            <option value="">-- Select --</option>
            @for (s of suppliers(); track s.id) {
              <option [value]="s.id">{{ s.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="warehouseId">Warehouse</label>
          <select id="warehouseId" [(ngModel)]="warehouseId" name="warehouseId" required>
            <option value="">-- Select --</option>
            @for (w of warehouses(); track w.id) {
              <option [value]="w.id">{{ w.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="expectedDeliveryDate">Expected Delivery Date</label>
          <input id="expectedDeliveryDate" type="date" [(ngModel)]="expectedDeliveryDate" name="expectedDeliveryDate" required />
        </div>
        <div class="form-group">
          <label for="notes">Notes</label>
          <input id="notes" type="text" [(ngModel)]="notes" name="notes" />
        </div>
        <div class="form-group">
          <label for="transportCompanyId">Transport Company</label>
          <select id="transportCompanyId" [(ngModel)]="transportCompanyId" name="transportCompanyId">
            <option value="">-- Select --</option>
            @for (tc of transportCompanies(); track tc.id) {
              <option [value]="tc.id">{{ tc.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="transportationCost">Transportation Cost</label>
          <input id="transportationCost" type="number" [(ngModel)]="transportationCost" name="transportationCost" />
        </div>
        <div class="form-group">
          <label for="dueDate">Due Date</label>
          <input id="dueDate" type="date" [(ngModel)]="dueDate" name="dueDate" />
        </div>
        <h4>Order Items</h4>
        @for (item of items; track $index) {
          <div class="item-row">
            <select [(ngModel)]="item.productId" [name]="'productId_' + $index" required>
              <option value="">-- Product --</option>
              @for (p of products(); track p.id) {
                <option [value]="p.id">{{ p.name }}</option>
              }
            </select>
            <input type="number" placeholder="Qty" [(ngModel)]="item.orderedQuantity" [name]="'qty_' + $index" required />
            <input type="number" placeholder="Price" [(ngModel)]="item.unitPrice" [name]="'price_' + $index" required step="0.01" />
            <button type="button" class="btn-remove" (click)="removeItem($index)">&times;</button>
          </div>
        }
        <button type="button" class="btn-add" (click)="addItem()">+ Add Item</button>
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="onClose()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">{{ saving() ? 'Saving...' : entityId ? 'Update' : 'Create' }}</button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group input, .form-group select { width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .item-row { display: flex; gap: 0.5rem; margin-bottom: 0.5rem; align-items: center; }
    .item-row select, .item-row input { flex: 1; }
    .btn-remove { background: #dc3545; color: white; border: none; border-radius: 4px; padding: 0.5rem; cursor: pointer; flex-shrink: 0; }
    .btn-add { background: #28a745; color: white; border: none; padding: 0.4rem 0.8rem; border-radius: 4px; cursor: pointer; margin-bottom: 1rem; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
    h4 { margin: 1rem 0 0.5rem; font-size: 0.95rem; color: #555; }
  `],
})
export class PurchaseOrderFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  supplierId = '';
  warehouseId = '';
  expectedDeliveryDate = '';
  notes = '';
  transportCompanyId = '';
  transportationCost = 0;
  dueDate = '';
  items: OrderItem[] = [{ productId: '', orderedQuantity: 0, unitPrice: 0 }];
  suppliers = signal<Supplier[]>([]);
  warehouses = signal<Warehouse[]>([]);
  products = signal<Product[]>([]);
  transportCompanies = signal<TransportCompany[]>([]);
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.resetForm();
      this.api.getAllList<Supplier>('suppliers').subscribe(data => this.suppliers.set(data));
      this.api.getAllList<Warehouse>('warehouses').subscribe(data => this.warehouses.set(data));
      this.api.getAllList<Product>('products').subscribe(data => this.products.set(data));
      this.api.getAllList<TransportCompany>('transportcompanies').subscribe(data => this.transportCompanies.set(data));
      if (this.entityId) {
        this.api.getById<any>('purchaseorders', this.entityId).subscribe(po => {
          this.supplierId = po.supplierId ?? '';
          this.warehouseId = po.warehouseId ?? '';
          this.expectedDeliveryDate = po.expectedDeliveryDate ?? '';
          this.notes = po.notes ?? '';
          this.transportCompanyId = po.transportCompanyId ?? '';
          this.transportationCost = po.transportationCost ?? 0;
          this.dueDate = po.dueDate ?? '';
        });
      }
    }
  }

  addItem() {
    this.items.push({ productId: '', orderedQuantity: 0, unitPrice: 0 });
  }

  removeItem(index: number) {
    if (this.items.length > 1) {
      this.items.splice(index, 1);
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      supplierId: this.supplierId,
      warehouseId: this.warehouseId,
      expectedDeliveryDate: this.expectedDeliveryDate,
      notes: this.notes,
      transportCompanyId: this.transportCompanyId,
      transportationCost: this.transportationCost,
      dueDate: this.dueDate,
      items: this.items.map(i => ({
        productId: i.productId,
        orderedQuantity: i.orderedQuantity,
        unitPrice: i.unitPrice,
      })),
    };

    const req$ = this.entityId
      ? this.api.update('purchaseorders', this.entityId, body)
      : this.api.create('purchaseorders', body);

    req$.subscribe({
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
    this.supplierId = '';
    this.warehouseId = '';
    this.expectedDeliveryDate = '';
    this.notes = '';
    this.transportCompanyId = '';
    this.transportationCost = 0;
    this.dueDate = '';
    this.items = [{ productId: '', orderedQuantity: 0, unitPrice: 0 }];
  }
}
