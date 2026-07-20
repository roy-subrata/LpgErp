import { Component, EventEmitter, Input, Output, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-supplier-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Supplier' : 'New Supplier'" (close)="close.emit()">
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
          <label for="contactPerson">Contact Person</label>
          <input id="contactPerson" type="text" [(ngModel)]="contactPerson" name="contactPerson" />
        </div>
        <div class="form-group">
          <label for="phone">Phone</label>
          <input id="phone" type="text" [(ngModel)]="phone" name="phone" />
        </div>
        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" type="email" [(ngModel)]="email" name="email" />
        </div>
        <div class="form-group">
          <label for="address">Address</label>
          <input id="address" type="text" [(ngModel)]="address" name="address" />
        </div>
        <div class="form-group">
          <label for="brandId">Brand</label>
          <select id="brandId" [(ngModel)]="brandId" name="brandId">
            <option value="">-- Select Brand --</option>
            @for (b of brands(); track b.id) {
              <option [value]="b.id">{{ b.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="isLpgCompany">LPG Company</label>
          <input id="isLpgCompany" type="checkbox" [(ngModel)]="isLpgCompany" name="isLpgCompany" />
        </div>
        <div class="form-group">
          <label for="isActive">Active</label>
          <input id="isActive" type="checkbox" [(ngModel)]="isActive" name="isActive" />
        </div>
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="close.emit()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving">{{ saving ? 'Saving...' : 'Save' }}</button>
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
export class SupplierFormComponent {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  name = '';
  code = '';
  contactPerson = '';
  phone = '';
  email = '';
  address = '';
  isLpgCompany = false;
  brandId = '';
  isActive = true;
  saving = false;
  brands = signal<any[]>([]);

  private prevOpen = false;

  constructor() {
    effect(() => {
      if (this.open && !this.prevOpen) {
        this.loadBrands();
        if (this.entityId) {
          this.loadEntity();
        } else {
          this.resetForm();
        }
      }
      this.prevOpen = this.open;
    });
  }

  private loadBrands() {
    this.api.getAllList<any>('brands').subscribe(data => this.brands.set(data));
  }

  private loadEntity() {
    this.api.getById<any>('suppliers', this.entityId!).subscribe(s => {
      this.name = s.name ?? '';
      this.code = s.code ?? '';
      this.contactPerson = s.contactPerson ?? '';
      this.phone = s.phone ?? '';
      this.email = s.email ?? '';
      this.address = s.address ?? '';
      this.isLpgCompany = s.isLpgCompany ?? false;
      this.brandId = s.brandId ?? '';
      this.isActive = s.isActive ?? true;
    });
  }

  private resetForm() {
    this.name = '';
    this.code = '';
    this.contactPerson = '';
    this.phone = '';
    this.email = '';
    this.address = '';
    this.isLpgCompany = false;
    this.brandId = '';
    this.isActive = true;
  }

  submit() {
    this.saving = true;
    const body = {
      name: this.name,
      code: this.code,
      contactPerson: this.contactPerson,
      phone: this.phone,
      email: this.email,
      address: this.address,
      isLpgCompany: this.isLpgCompany,
      brandId: this.brandId || null,
      isActive: this.isActive,
    };

    const obs = this.entityId
      ? this.api.update('suppliers', this.entityId, body)
      : this.api.create('suppliers', body);

    obs.subscribe({
      next: () => {
        this.saving = false;
        this.saved.emit();
      },
      error: () => {
        this.saving = false;
      },
    });
  }
}
