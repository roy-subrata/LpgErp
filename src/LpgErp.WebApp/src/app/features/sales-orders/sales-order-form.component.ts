import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Customer, Warehouse, Product, TransportCompany } from '../../core/models';

interface SalesItem {
  productId: string;
  quantity: number;
  unitPrice: number;
  cylinderExchangeQuantity: number;
  /** Empties handed over for refill lines. null = full swap (= qty); 0 = advance refill (cylinder owed). */
  emptyReturnedQuantity: number | null;
}

@Component({
  selector: 'app-sales-order-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Sales Order' : 'New Sales Order'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="customerId">Customer</label>
          <select id="customerId" [(ngModel)]="customerId" name="customerId" required>
            <option value="">-- Select --</option>
            @for (c of customers(); track c.id) {
              <option [value]="c.id">{{ c.name }}</option>
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
          <label for="routeId">Route</label>
          <select id="routeId" [(ngModel)]="routeId" name="routeId">
            <option value="">-- None --</option>
            @for (r of routes(); track r.id) {
              <option [value]="r.id">{{ r.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="vehicleLoadingId">Vehicle Loading (mobile sale)</label>
          <select id="vehicleLoadingId" [(ngModel)]="vehicleLoadingId" name="vehicleLoadingId">
            <option value="">-- None (direct warehouse sale) --</option>
            @for (vl of vehicleLoadings(); track vl.id) {
              <option [value]="vl.id">{{ vl.truckName }} · {{ vl.loadingDate?.slice(0, 10) }}{{ vl.routeName ? ' · ' + vl.routeName : '' }}</option>
            }
          </select>
          <small class="field-hint">When set, delivery draws down the vehicle's loaded stock instead of warehouse stock.</small>
        </div>
        <div class="form-group checkbox-group">
          <label>
            <input type="checkbox" [(ngModel)]="isCreditSale" name="isCreditSale" />
            Credit Sale
          </label>
        </div>
        <div class="form-group">
          <label for="notes">Notes</label>
          <input id="notes" type="text" [(ngModel)]="notes" name="notes" />
        </div>
        <div class="form-group">
          <label for="dueDate">Due Date</label>
          <input id="dueDate" type="date" [(ngModel)]="dueDate" name="dueDate" />
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
        <h4>Order Items</h4>
        @for (item of items; track $index) {
          <div class="item-row">
            <select [(ngModel)]="item.productId" [name]="'productId_' + $index" required>
              <option value="">-- Product --</option>
              @for (p of products(); track p.id) {
                <option [value]="p.id">{{ p.name }}</option>
              }
            </select>
            <input type="number" placeholder="Qty" [(ngModel)]="item.quantity" [name]="'qty_' + $index" required />
            <input type="number" placeholder="Price" [(ngModel)]="item.unitPrice" [name]="'price_' + $index" required step="0.01" />
            <input type="number" placeholder="Cyl Exch" [(ngModel)]="item.cylinderExchangeQuantity" [name]="'cex_' + $index" />
            <input type="number" min="0" placeholder="Empty Rtn (= qty)" title="Empties returned for refills. Blank = full swap; 0 = advance refill (cylinder owed)." [(ngModel)]="item.emptyReturnedQuantity" [name]="'ert_' + $index" />
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
    .checkbox-group label { display: flex; align-items: center; gap: 0.5rem; font-weight: 400; }
    .field-hint { display: block; margin-top: 0.25rem; font-size: 0.75rem; color: #6b7280; }
    .checkbox-group input[type="checkbox"] { width: auto; }
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
export class SalesOrderFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  customerId = '';
  warehouseId = '';
  routeId = '';
  vehicleLoadingId = '';
  isCreditSale = false;
  notes = '';
  dueDate = '';
  transportCompanyId = '';
  items: SalesItem[] = [{ productId: '', quantity: 0, unitPrice: 0, cylinderExchangeQuantity: 0, emptyReturnedQuantity: null }];
  customers = signal<Customer[]>([]);
  warehouses = signal<Warehouse[]>([]);
  products = signal<Product[]>([]);
  transportCompanies = signal<TransportCompany[]>([]);
  routes = signal<any[]>([]);
  vehicleLoadings = signal<any[]>([]);
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.resetForm();
      this.api.getAllList<Customer>('customers').subscribe(data => this.customers.set(data));
      this.api.getAllList<Warehouse>('warehouses').subscribe(data => this.warehouses.set(data));
      this.api.getAllList<Product>('products').subscribe(data => this.products.set(data));
      this.api.getAllList<TransportCompany>('transportcompanies').subscribe(data => this.transportCompanies.set(data));
      this.api.getAllList<any>('routes').subscribe(data => this.routes.set(data));
      // Only dispatched loadings (status 0) can receive mobile sales.
      this.api.getAll<any>('vehicleloadings', 1, 100).subscribe(page =>
        this.vehicleLoadings.set(page.items.filter((vl: any) => vl.status === 0)));
      if (this.entityId) {
        this.api.getById<any>('salesorders', this.entityId).subscribe(so => {
          this.customerId = so.customerId ?? '';
          this.warehouseId = so.warehouseId ?? '';
          this.routeId = so.routeId ?? '';
          this.vehicleLoadingId = so.vehicleLoadingId ?? '';
          this.isCreditSale = so.isCreditSale ?? false;
          this.notes = so.notes ?? '';
          this.dueDate = so.dueDate?.split('T')[0] ?? '';
          this.transportCompanyId = so.transportCompanyId ?? '';
          if (so.items?.length) {
            this.items = so.items.map((i: any) => ({
              productId: i.productId,
              quantity: i.quantity,
              unitPrice: i.unitPrice,
              cylinderExchangeQuantity: i.cylinderExchangeQuantity ?? 0,
              emptyReturnedQuantity: i.emptyReturnedQuantity ?? null,
            }));
          }
        });
      }
    }
  }

  addItem() {
    this.items.push({ productId: '', quantity: 0, unitPrice: 0, cylinderExchangeQuantity: 0, emptyReturnedQuantity: null });
  }

  removeItem(index: number) {
    if (this.items.length > 1) {
      this.items.splice(index, 1);
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      customerId: this.customerId,
      warehouseId: this.warehouseId,
      routeId: this.routeId || null,
      vehicleLoadingId: this.vehicleLoadingId || null,
      isCreditSale: this.isCreditSale,
      notes: this.notes,
      dueDate: this.dueDate,
      transportCompanyId: this.transportCompanyId,
      items: this.items.map(i => ({
        productId: i.productId,
        quantity: i.quantity,
        unitPrice: i.unitPrice,
        cylinderExchangeQuantity: i.cylinderExchangeQuantity,
        emptyReturnedQuantity: i.emptyReturnedQuantity === null || (i.emptyReturnedQuantity as any) === '' ? null : i.emptyReturnedQuantity,
      })),
    };

    const req$ = this.entityId
      ? this.api.update('salesorders', this.entityId, body)
      : this.api.create('salesorders', body);

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
    this.customerId = '';
    this.warehouseId = '';
    this.routeId = '';
    this.vehicleLoadingId = '';
    this.isCreditSale = false;
    this.notes = '';
    this.dueDate = '';
    this.transportCompanyId = '';
    this.items = [{ productId: '', quantity: 0, unitPrice: 0, cylinderExchangeQuantity: 0, emptyReturnedQuantity: null }];
  }
}
