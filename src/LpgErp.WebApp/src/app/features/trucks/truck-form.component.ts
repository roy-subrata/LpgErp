import { Component, EventEmitter, Input, Output, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-truck-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Truck' : 'New Truck'" (close)="close.emit()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="name">Name</label>
          <input id="name" type="text" [(ngModel)]="name" name="name" required />
        </div>
        <div class="form-group">
          <label for="plateNumber">Plate Number</label>
          <input id="plateNumber" type="text" [(ngModel)]="plateNumber" name="plateNumber" required />
        </div>
        <div class="form-group">
          <label for="capacity">Capacity</label>
          <input id="capacity" type="number" [(ngModel)]="capacity" name="capacity" required />
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
export class TruckFormComponent {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  name = '';
  plateNumber = '';
  capacity = 0;
  isActive = true;
  saving = false;

  private prevOpen = false;

  constructor() {
    effect(() => {
      if (this.open && !this.prevOpen) {
        if (this.entityId) {
          this.loadEntity();
        } else {
          this.resetForm();
        }
      }
      this.prevOpen = this.open;
    });
  }

  private loadEntity() {
    this.api.getById<any>('trucks', this.entityId!).subscribe(t => {
      this.name = t.name ?? '';
      this.plateNumber = t.plateNumber ?? '';
      this.capacity = t.capacity ?? 0;
      this.isActive = t.isActive ?? true;
    });
  }

  private resetForm() {
    this.name = '';
    this.plateNumber = '';
    this.capacity = 0;
    this.isActive = true;
  }

  submit() {
    this.saving = true;
    const body = {
      name: this.name,
      plateNumber: this.plateNumber,
      capacity: this.capacity,
      isActive: this.isActive,
    };

    const obs = this.entityId
      ? this.api.update('trucks', this.entityId, body)
      : this.api.create('trucks', body);

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
