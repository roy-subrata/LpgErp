import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Brand, Customer, CylinderSize, CylinderExchange } from '../../core/models';

@Component({
  selector: 'app-cylinder-exchange-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Cylinder Exchange' : 'New Cylinder Exchange'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="customerId">Customer</label>
          <select id="customerId" [(ngModel)]="customerId" name="customerId" required>
            <option value="">Select Customer</option>
            @for (customer of customers(); track customer.id) {
              <option [value]="customer.id">{{ customer.name }}</option>
            }
          </select>
        </div>
        <h4>Incoming Cylinder</h4>
        <div class="form-group">
          <label for="incomingBrandId">Brand</label>
          <select id="incomingBrandId" [(ngModel)]="incomingBrandId" name="incomingBrandId" required>
            <option value="">Select Brand</option>
            @for (brand of brands(); track brand.id) {
              <option [value]="brand.id">{{ brand.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="incomingCylinderSizeId">Cylinder Size</label>
          <select id="incomingCylinderSizeId" [(ngModel)]="incomingCylinderSizeId" name="incomingCylinderSizeId" required>
            <option value="">Select Cylinder Size</option>
            @for (size of cylinderSizes(); track size.id) {
              <option [value]="size.id">{{ size.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="incomingQuantity">Quantity</label>
          <input id="incomingQuantity" type="number" [(ngModel)]="incomingQuantity" name="incomingQuantity" required />
        </div>
        <h4>Outgoing Cylinder</h4>
        <div class="form-group">
          <label for="outgoingBrandId">Brand</label>
          <select id="outgoingBrandId" [(ngModel)]="outgoingBrandId" name="outgoingBrandId" required>
            <option value="">Select Brand</option>
            @for (brand of brands(); track brand.id) {
              <option [value]="brand.id">{{ brand.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="outgoingCylinderSizeId">Cylinder Size</label>
          <select id="outgoingCylinderSizeId" [(ngModel)]="outgoingCylinderSizeId" name="outgoingCylinderSizeId" required>
            <option value="">Select Cylinder Size</option>
            @for (size of cylinderSizes(); track size.id) {
              <option [value]="size.id">{{ size.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="outgoingQuantity">Quantity</label>
          <input id="outgoingQuantity" type="number" [(ngModel)]="outgoingQuantity" name="outgoingQuantity" required />
        </div>
        <div class="form-group">
          <label for="exchangeCharge">Exchange Charge</label>
          <input id="exchangeCharge" type="number" [(ngModel)]="exchangeCharge" name="exchangeCharge" required />
        </div>
        <div class="form-group">
          <label for="notes">Notes</label>
          <input id="notes" type="text" [(ngModel)]="notes" name="notes" />
        </div>
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="onClose()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">Save</button>
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
    h4 { margin: 1rem 0 0.5rem; font-size: 0.95rem; color: #555; }
  `],
})
export class CylinderExchangeFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  customerId = '';
  incomingBrandId = '';
  incomingCylinderSizeId = '';
  incomingQuantity = 0;
  outgoingBrandId = '';
  outgoingCylinderSizeId = '';
  outgoingQuantity = 0;
  exchangeCharge = 0;
  notes = '';
  customers = signal<Customer[]>([]);
  brands = signal<Brand[]>([]);
  cylinderSizes = signal<CylinderSize[]>([]);
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.api.getAllList<Customer>('customers').subscribe(data => this.customers.set(data));
      this.api.getAllList<Brand>('brands').subscribe(data => this.brands.set(data));
      this.api.getAllList<CylinderSize>('cylindersizes').subscribe(data => this.cylinderSizes.set(data));
      if (this.entityId) {
        this.api.getById<CylinderExchange>('cylinderexchanges', this.entityId).subscribe(exchange => {
          this.customerId = exchange.customerId;
          this.incomingBrandId = exchange.incomingBrandId;
          this.incomingCylinderSizeId = exchange.incomingCylinderSizeId;
          this.incomingQuantity = exchange.incomingQuantity;
          this.outgoingBrandId = exchange.outgoingBrandId;
          this.outgoingCylinderSizeId = exchange.outgoingCylinderSizeId;
          this.outgoingQuantity = exchange.outgoingQuantity;
          this.exchangeCharge = exchange.exchangeCharge;
          this.notes = exchange.notes;
        });
      } else {
        this.resetForm();
      }
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      customerId: this.customerId,
      incomingBrandId: this.incomingBrandId,
      incomingCylinderSizeId: this.incomingCylinderSizeId,
      incomingQuantity: this.incomingQuantity,
      outgoingBrandId: this.outgoingBrandId,
      outgoingCylinderSizeId: this.outgoingCylinderSizeId,
      outgoingQuantity: this.outgoingQuantity,
      exchangeCharge: this.exchangeCharge,
      notes: this.notes,
    };

    const req$ = this.entityId
      ? this.api.update('cylinderexchanges', this.entityId, body)
      : this.api.create('cylinderexchanges', body);

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
    this.incomingBrandId = '';
    this.incomingCylinderSizeId = '';
    this.incomingQuantity = 0;
    this.outgoingBrandId = '';
    this.outgoingCylinderSizeId = '';
    this.outgoingQuantity = 0;
    this.exchangeCharge = 0;
    this.notes = '';
  }
}
