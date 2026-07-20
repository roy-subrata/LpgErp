import { Component, EventEmitter, Input, Output, inject, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-route-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Route' : 'New Route'" (close)="close.emit()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="name">Name</label>
          <input id="name" type="text" [(ngModel)]="name" name="name" required />
        </div>
        <div class="form-group">
          <label for="area">Area</label>
          <input id="area" type="text" [(ngModel)]="area" name="area" />
        </div>
        <div class="form-group">
          <label for="description">Description</label>
          <input id="description" type="text" [(ngModel)]="description" name="description" />
        </div>
        <div class="form-group">
          <label for="village">Village</label>
          <input id="village" type="text" [(ngModel)]="village" name="village" />
        </div>
        <div class="form-group">
          <label for="dealer">Dealer</label>
          <input id="dealer" type="text" [(ngModel)]="dealer" name="dealer" />
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
export class RouteFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  name = '';
  area = '';
  description = '';
  village = '';
  dealer = '';
  isActive = true;
  saving = false;

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      if (this.entityId) {
        this.loadEntity();
      } else {
        this.resetForm();
      }
    }
  }

  private loadEntity() {
    this.api.getById<any>('routes', this.entityId!).subscribe(r => {
      this.name = r.name ?? '';
      this.area = r.area ?? '';
      this.description = r.description ?? '';
      this.village = r.village ?? '';
      this.dealer = r.dealer ?? '';
      this.isActive = r.isActive ?? true;
    });
  }

  private resetForm() {
    this.name = '';
    this.area = '';
    this.description = '';
    this.village = '';
    this.dealer = '';
    this.isActive = true;
  }

  submit() {
    this.saving = true;
    const body = {
      name: this.name,
      area: this.area,
      description: this.description,
      village: this.village,
      dealer: this.dealer,
      isActive: this.isActive,
    };

    const obs = this.entityId
      ? this.api.update('routes', this.entityId, body)
      : this.api.create('routes', body);

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
