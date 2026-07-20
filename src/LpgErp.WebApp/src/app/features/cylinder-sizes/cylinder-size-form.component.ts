import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Brand, CylinderSize } from '../../core/models';

@Component({
  selector: 'app-cylinder-size-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Cylinder Size' : 'New Cylinder Size'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="name">Name</label>
          <input id="name" type="text" [(ngModel)]="name" name="name" required />
        </div>
        <div class="form-group">
          <label for="brandId">Brand</label>
          <select id="brandId" [(ngModel)]="brandId" name="brandId" required>
            <option value="">Select Brand</option>
            @for (brand of brands(); track brand.id) {
              <option [value]="brand.id">{{ brand.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="weightKg">Weight (Kg)</label>
          <input id="weightKg" type="number" [(ngModel)]="weightKg" name="weightKg" required />
        </div>
        <div class="form-group">
          <label for="depositAmount">Deposit Amount</label>
          <input id="depositAmount" type="number" [(ngModel)]="depositAmount" name="depositAmount" required />
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
export class CylinderSizeFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  name = '';
  brandId = '';
  weightKg = 0;
  depositAmount = 0;
  isActive = true;
  brands = signal<Brand[]>([]);
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.api.getAllList<Brand>('brands').subscribe(data => this.brands.set(data));
      if (this.entityId) {
        this.api.getById<CylinderSize>('cylindersizes', this.entityId).subscribe(size => {
          this.name = size.name;
          this.brandId = size.brandId;
          this.weightKg = size.weightKg;
          this.depositAmount = size.depositAmount;
          this.isActive = size.isActive;
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
      brandId: this.brandId,
      weightKg: this.weightKg,
      depositAmount: this.depositAmount,
      isActive: this.isActive,
    };

    const req$ = this.entityId
      ? this.api.update('cylindersizes', this.entityId, body)
      : this.api.create('cylindersizes', body);

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
    this.name = '';
    this.brandId = '';
    this.weightKg = 0;
    this.depositAmount = 0;
    this.isActive = true;
  }
}
