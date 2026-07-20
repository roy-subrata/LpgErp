import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { Cylinder } from '../../core/models';
import { CylinderFormComponent } from './cylinder-form.component';

@Component({
  selector: 'app-cylinder-list',
  standalone: true,
  imports: [CommonModule, CylinderFormComponent],
  template: `
    <div class="page-header">
      <h1>Cylinders</h1>
      <button class="btn-primary" (click)="showForm.set(true)">+ New Cylinder</button>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Serial Number</th>
            <th>Brand</th>
            <th>Size</th>
            <th>Status</th>
            <th>Has Gas</th>
            <th>Warehouse</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (c of items(); track c.id) {
            <tr>
              <td>{{ c.serialNumber }}</td>
              <td>{{ c.brandName }}</td>
              <td>{{ c.cylinderSizeName }}</td>
              <td>{{ cylinderStatus(c.status) }}</td>
              <td>{{ c.hasGas ? 'Yes' : 'No' }}</td>
              <td>{{ c.currentWarehouseName }}</td>
              <td>
                <button class="btn-sm" (click)="editId.set(c.id); showForm.set(true)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(c.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="7">No cylinders found.</td></tr>
          }
        </tbody>
      </table>
    </div>
    <app-cylinder-form [open]="showForm()" [entityId]="editId()" (close)="closeForm()" (saved)="loadData()" />
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
export class CylinderListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<Cylinder[]>([]);
  showForm = signal(false);
  editId = signal<string | null>(null);

  ngOnInit() { this.loadData(); }

  loadData() {
    this.api.getAllList<Cylinder>('cylinders').subscribe(data => this.items.set(data));
  }

  cylinderStatus(status: number): string {
    const map: Record<number, string> = { 0: 'Available', 1: 'WithCustomer', 2: 'InTransit', 3: 'Damaged', 4: 'UnderMaintenance' };
    return map[status] ?? 'Unknown';
  }

  editCylinder(id: string) {
    this.editId.set(id);
    this.showForm.set(true);
  }

  closeForm() {
    this.showForm.set(false);
    this.editId.set(null);
  }

  onDelete(id: string) {
    if (confirm('Are you sure?')) {
      this.api.delete('cylinders', id).subscribe(() => this.loadData());
    }
  }
}
