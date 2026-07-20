import { Component, EventEmitter, Input, Output, inject, signal, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Brand, CylinderSize, Warehouse } from '../../core/models';

@Component({
  selector: 'app-cylinder-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Cylinder' : 'New Cylinder'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label>Serial Number</label>
          <input type="text" [(ngModel)]="serialNumber" name="serialNumber" required />
        </div>
        <div class="form-group">
          <label>Brand</label>
          <select [(ngModel)]="brandId" name="brandId" required>
            <option value="">Select Brand</option>
            @for (b of brands(); track b.id) {
              <option [value]="b.id">{{ b.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>Cylinder Size</label>
          <select [(ngModel)]="cylinderSizeId" name="cylinderSizeId" required>
            <option value="">Select Size</option>
            @for (s of sizes(); track s.id) {
              <option [value]="s.id">{{ s.name }} ({{ s.weightKg }}kg)</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>Warehouse</label>
          <select [(ngModel)]="currentWarehouseId" name="currentWarehouseId">
            <option value="">Select Warehouse</option>
            @for (w of warehouses(); track w.id) {
              <option [value]="w.id">{{ w.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>Status</label>
          <select [(ngModel)]="status" name="status">
            <option [ngValue]="0">Available</option>
            <option [ngValue]="1">With Customer</option>
            <option [ngValue]="2">In Transit</option>
            <option [ngValue]="3">Damaged</option>
            <option [ngValue]="4">Under Maintenance</option>
          </select>
        </div>
        <div class="form-group">
          <label>
            <input type="checkbox" [(ngModel)]="hasGas" name="hasGas" />
            Has Gas
          </label>
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
    .form-group input[type="checkbox"] { width: auto; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class CylinderFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  brands = signal<Brand[]>([]);
  sizes = signal<CylinderSize[]>([]);
  warehouses = signal<Warehouse[]>([]);
  serialNumber = '';
  brandId = '';
  cylinderSizeId = '';
  currentWarehouseId = '';
  status = 0;
  hasGas = false;
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.api.getAllList<Brand>('brands').subscribe(b => this.brands.set(b));
      this.api.getAllList<CylinderSize>('cylindersizes').subscribe(s => this.sizes.set(s));
      this.api.getAllList<Warehouse>('warehouses').subscribe(w => this.warehouses.set(w));

      if (this.entityId) {
        this.api.getById<any>('cylinders', this.entityId).subscribe(c => {
          this.serialNumber = c.serialNumber;
          this.brandId = c.brandId;
          this.cylinderSizeId = c.cylinderSizeId;
          this.currentWarehouseId = c.currentWarehouseId || '';
          this.status = c.status;
          this.hasGas = c.hasGas;
        });
      } else {
        this.resetForm();
      }
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      serialNumber: this.serialNumber,
      brandId: this.brandId,
      cylinderSizeId: this.cylinderSizeId,
      currentWarehouseId: this.currentWarehouseId || null,
      status: +this.status,
      hasGas: this.hasGas,
    };

    const req$ = this.entityId
      ? this.api.update('cylinders', this.entityId, body)
      : this.api.create('cylinders', body);

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
    this.serialNumber = '';
    this.brandId = '';
    this.cylinderSizeId = '';
    this.currentWarehouseId = '';
    this.status = 0;
    this.hasGas = false;
  }
}
