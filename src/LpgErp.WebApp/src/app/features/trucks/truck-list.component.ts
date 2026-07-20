import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { Truck } from '../../core/models';
import { TruckFormComponent } from './truck-form.component';

@Component({
  selector: 'app-truck-list',
  standalone: true,
  imports: [CommonModule, TruckFormComponent],
  template: `
    <div class="page-header">
      <h1>Trucks</h1>
      <button class="btn-primary" (click)="onNew()">+ New Truck</button>
    </div>
    <app-truck-form [open]="showForm()" [entityId]="editingId()" (saved)="onSaved()" (close)="showForm.set(false)" />
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Plate Number</th>
            <th>Capacity</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (t of items(); track t.id) {
            <tr>
              <td>{{ t.name }}</td>
              <td>{{ t.plateNumber }}</td>
              <td>{{ t.capacity }}</td>
              <td>{{ t.isActive ? 'Active' : 'Inactive' }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(t.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(t.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="5">No trucks found.</td></tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .btn-primary { background: #1a1a2e; color: white; border: none; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #eee; }
    th { background: #f8f9fa; font-weight: 600; }
    .btn-sm { padding: 0.25rem 0.5rem; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; margin-right: 0.25rem; }
    .btn-danger { color: #dc3545; border-color: #dc3545; }
  `],
})
export class TruckListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<Truck[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAllList<Truck>('trucks').subscribe(data => this.items.set(data));
  }

  onNew() {
    this.editingId.set(null);
    this.showForm.set(true);
  }

  onEdit(id: string) {
    this.editingId.set(id);
    this.showForm.set(true);
  }

  onDelete(id: string) {
    if (confirm('Are you sure?')) {
      this.api.delete('trucks', id).subscribe(() => this.loadData());
    }
  }

  onSaved() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.loadData();
  }
}
