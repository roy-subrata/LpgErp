import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Warehouse } from '../../core/models';

@Component({
  selector: 'app-warehouse-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Warehouse' : 'New Warehouse'" (close)="onClose()">
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
          <label for="address">Address</label>
          <input id="address" type="text" [(ngModel)]="address" name="address" />
        </div>
        <div class="form-group">
          <label for="phone">Phone</label>
          <input id="phone" type="text" [(ngModel)]="phone" name="phone" />
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
export class WarehouseFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  name = '';
  code = '';
  address = '';
  phone = '';
  isActive = true;
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      if (this.entityId) {
        this.api.getById<Warehouse>('warehouses', this.entityId).subscribe(warehouse => {
          this.name = warehouse.name;
          this.code = warehouse.code;
          this.address = warehouse.address;
          this.phone = warehouse.phone;
          this.isActive = warehouse.isActive;
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
      address: this.address,
      phone: this.phone,
      isActive: this.isActive,
    };

    const req$ = this.entityId
      ? this.api.update('warehouses', this.entityId, body)
      : this.api.create('warehouses', body);

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
    this.code = '';
    this.address = '';
    this.phone = '';
    this.isActive = true;
  }
}
