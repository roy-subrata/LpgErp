import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Product, Brand, CylinderSize } from '../../core/models';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Product' : 'New Product'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="name">Name</label>
          <input id="name" type="text" [(ngModel)]="name" name="name" required />
        </div>
        <div class="form-group">
          <label for="code">Code</label>
          <input id="code" type="text" [(ngModel)]="code" name="code" required />
        </div>
        <div class="form-group">
          <label for="type">Type</label>
          <select id="type" [(ngModel)]="type" name="type">
            <option [ngValue]="0">Empty Cylinder</option>
            <option [ngValue]="1">Gas Refill</option>
            <option [ngValue]="2">New Package</option>
            <option [ngValue]="3">Accessory</option>
          </select>
        </div>
        <div class="form-group">
          <label for="brandId">Brand</label>
          <select id="brandId" [(ngModel)]="brandId" name="brandId">
            <option value="">-- Select Brand --</option>
            @for (brand of brands(); track brand.id) {
              <option [value]="brand.id">{{ brand.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="cylinderSizeId">Cylinder Size</label>
          <select id="cylinderSizeId" [(ngModel)]="cylinderSizeId" name="cylinderSizeId">
            <option value="">-- Select Size --</option>
            @for (size of cylinderSizes(); track size.id) {
              <option [value]="size.id">{{ size.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="purchasePrice">Purchase Price</label>
          <input id="purchasePrice" type="number" [(ngModel)]="purchasePrice" name="purchasePrice" />
        </div>
        <div class="form-group">
          <label for="salePrice">Sale Price</label>
          <input id="salePrice" type="number" [(ngModel)]="salePrice" name="salePrice" />
        </div>
        <div class="form-group">
          <label for="currentStock">Current Stock</label>
          <input id="currentStock" type="number" [(ngModel)]="currentStock" name="currentStock" />
        </div>
        <div class="form-group">
          <label for="minimumStock">Minimum Stock</label>
          <input id="minimumStock" type="number" [(ngModel)]="minimumStock" name="minimumStock" />
        </div>
        <div class="form-group">
          <label>
            <input type="checkbox" [(ngModel)]="isActive" name="isActive" />
            Active
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
export class ProductFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  name = '';
  code = '';
  type = 0;
  brandId = '';
  cylinderSizeId = '';
  purchasePrice = 0;
  salePrice = 0;
  currentStock = 0;
  minimumStock = 0;
  isActive = true;
  saving = signal(false);
  brands = signal<Brand[]>([]);
  cylinderSizes = signal<CylinderSize[]>([]);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.loadDropdowns();
      if (this.entityId) {
        this.api.getById<Product>('products', this.entityId).subscribe(product => {
          this.name = product.name;
          this.code = product.code;
          this.type = product.type;
          this.brandId = product.brandId;
          this.cylinderSizeId = product.cylinderSizeId;
          this.purchasePrice = product.purchasePrice;
          this.salePrice = product.salePrice;
          this.currentStock = product.currentStock;
          this.minimumStock = product.minimumStock;
          this.isActive = product.isActive;
        });
      } else {
        this.resetForm();
      }
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      name: this.name,
      code: this.code,
      type: Number(this.type),
      brandId: this.brandId,
      cylinderSizeId: this.cylinderSizeId,
      purchasePrice: this.purchasePrice,
      salePrice: this.salePrice,
      currentStock: this.currentStock,
      minimumStock: this.minimumStock,
      isActive: this.isActive,
    };

    const req$ = this.entityId
      ? this.api.update('products', this.entityId, body)
      : this.api.create('products', body);

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

  private loadDropdowns() {
    this.api.getAllList<Brand>('brands').subscribe(data => this.brands.set(data));
    this.api.getAllList<CylinderSize>('cylindersizes').subscribe(data => this.cylinderSizes.set(data));
  }

  private resetForm() {
    this.name = '';
    this.code = '';
    this.type = 0;
    this.brandId = '';
    this.cylinderSizeId = '';
    this.purchasePrice = 0;
    this.salePrice = 0;
    this.currentStock = 0;
    this.minimumStock = 0;
    this.isActive = true;
  }
}
