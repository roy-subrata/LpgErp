import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { Warehouse } from '../../core/models';
import { WarehouseFormComponent } from './warehouse-form.component';

@Component({
  selector: 'app-warehouse-list',
  standalone: true,
  imports: [CommonModule, WarehouseFormComponent],
  template: `
    <div class="page-header">
      <h1>Warehouses</h1>
      <button class="btn-primary" (click)="openCreate()">+ New Warehouse</button>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Code</th>
            <th>Address</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (warehouse of warehouses(); track warehouse.id) {
            <tr>
              <td>{{ warehouse.name }}</td>
              <td>{{ warehouse.code }}</td>
              <td>{{ warehouse.address }}</td>
              <td>{{ warehouse.isActive ? 'Active' : 'Inactive' }}</td>
              <td>
                <button class="btn-sm" (click)="openEdit(warehouse.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(warehouse.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr>
              <td colspan="5">No warehouses found.</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
    <app-warehouse-form [open]="showForm()" [entityId]="editingId()" (close)="showForm.set(false)" (saved)="showForm.set(false); loadData()" />
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }
    .btn-primary {
      background: #1a1a2e;
      color: white;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
    }
    .table-container {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      overflow: hidden;
    }
    table {
      width: 100%;
      border-collapse: collapse;
    }
    th, td {
      padding: 0.75rem 1rem;
      text-align: left;
      border-bottom: 1px solid #eee;
    }
    th {
      background: #f8f9fa;
      font-weight: 600;
    }
    .btn-sm {
      padding: 0.25rem 0.5rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      cursor: pointer;
      margin-right: 0.25rem;
    }
    .btn-danger {
      color: #dc3545;
      border-color: #dc3545;
    }
  `],
})
export class WarehouseListComponent implements OnInit {
  private api = inject(ApiService);
  warehouses = signal<Warehouse[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAllList<Warehouse>('warehouses').subscribe(data => this.warehouses.set(data));
  }

  openCreate() {
    this.editingId.set(null);
    this.showForm.set(true);
  }

  openEdit(id: string) {
    this.editingId.set(id);
    this.showForm.set(true);
  }

  onDelete(id: string) {
    if (confirm('Are you sure?')) {
      this.api.delete('warehouses', id).subscribe(() => this.loadData());
    }
  }
}
