import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { VehicleLoading } from '../../core/models';
import { VehicleLoadingFormComponent } from './vehicle-loading-form.component';

@Component({
  selector: 'app-vehicle-loading-list',
  standalone: true,
  imports: [CommonModule, VehicleLoadingFormComponent],
  template: `
    <div class="page-header">
      <h1>Vehicle Loadings</h1>
      <button class="btn-primary" (click)="onNew()">+ New Loading</button>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Truck</th>
            <th>Driver</th>
            <th>Salesman</th>
            <th>Warehouse</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (v of items(); track v.id) {
            <tr>
              <td>{{ v.loadingDate | date:'short' }}</td>
              <td>{{ v.truckName }}</td>
              <td>{{ v.driverName }}</td>
              <td>{{ v.salesmanName }}</td>
              <td>{{ v.warehouseName }}</td>
              <td>{{ v.status === 0 ? 'Dispatched' : 'Returned' }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(v.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(v.id)">Delete</button>
                @if (v.status === 0) {
                  <button class="btn-sm btn-success" (click)="onClose(v.id)">Close</button>
                }
              </td>
            </tr>
          } @empty {
            <tr><td colspan="7">No vehicle loadings found.</td></tr>
          }
        </tbody>
      </table>
    </div>
    <app-vehicle-loading-form [open]="showForm()" [entityId]="editingId()" (close)="showForm.set(false)" (saved)="onSaved()" />
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .btn-primary { background: #1a1a2e; color: white; border: none; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; }
    .table-container { background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.75rem 1rem; text-align: left; border-bottom: 1px solid #eee; }
    th { background: #f8f9fa; font-weight: 600; }
    .btn-sm { padding: 0.25rem 0.5rem; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; margin-right: 0.25rem; }
    .btn-danger { border-color: #dc3545; color: #dc3545; }
    .btn-success { border-color: #28a745; color: #28a745; }
  `],
})
export class VehicleLoadingListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<VehicleLoading[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<VehicleLoading>('vehicleloadings').subscribe(data => this.items.set(data.items));
  }

  onNew() {
    this.editingId.set(null);
    this.showForm.set(true);
  }

  onEdit(id: string) {
    this.editingId.set(id);
    this.showForm.set(true);
  }

  onSaved() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.loadData();
  }

  onClose(id: string) {
    if (confirm('Close this vehicle loading? This will mark it as returned.')) {
      this.api.post('vehicleloadings/' + id + '/close', { items: [] }).subscribe(() => this.loadData());
    }
  }

  onDelete(id: string) {
    if (confirm('Are you sure you want to delete this vehicle loading?')) {
      this.api.delete('vehicleloadings', id).subscribe(() => this.loadData());
    }
  }
}
