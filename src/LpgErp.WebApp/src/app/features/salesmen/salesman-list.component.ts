import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { Salesman } from '../../core/models';
import { SalesmanFormComponent } from './salesman-form.component';

@Component({
  selector: 'app-salesman-list',
  standalone: true,
  imports: [CommonModule, SalesmanFormComponent],
  template: `
    <div class="page-header">
      <h1>Salesmen</h1>
      <button class="btn-primary" (click)="onNew()">+ New Salesman</button>
    </div>
    <app-salesman-form [open]="showForm()" [entityId]="editingId()" (saved)="onSaved()" (close)="showForm.set(false)" />
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Code</th>
            <th>Phone</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (s of items(); track s.id) {
            <tr>
              <td>{{ s.name }}</td>
              <td>{{ s.code }}</td>
              <td>{{ s.phone }}</td>
              <td>{{ s.isActive ? 'Active' : 'Inactive' }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(s.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(s.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="5">No salesmen found.</td></tr>
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
export class SalesmanListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<Salesman[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAllList<Salesman>('salesmen').subscribe(data => this.items.set(data));
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
      this.api.delete('salesmen', id).subscribe(() => this.loadData());
    }
  }

  onSaved() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.loadData();
  }
}
