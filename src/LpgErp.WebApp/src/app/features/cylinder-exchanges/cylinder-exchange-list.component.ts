import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { CylinderExchange } from '../../core/models';
import { CylinderExchangeFormComponent } from './cylinder-exchange-form.component';

@Component({
  selector: 'app-cylinder-exchange-list',
  standalone: true,
  imports: [CommonModule, CylinderExchangeFormComponent],
  template: `
    <div class="page-header">
      <h1>Cylinder Exchanges</h1>
      <button class="btn-primary" (click)="onNew()">+ New Exchange</button>
    </div>
    @if (showForm()) {
      <app-cylinder-exchange-form [entityId]="editingId()" (saved)="onSaved()" (close)="showForm.set(false)" />
    }
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Customer</th>
            <th>Incoming</th>
            <th>Outgoing</th>
            <th>Charge</th>
            <th>Notes</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (e of items(); track e.id) {
            <tr>
              <td>{{ e.exchangeDate | date:'short' }}</td>
              <td>{{ e.customerName }}</td>
              <td>{{ e.incomingQuantity }}x {{ e.incomingBrandName }} {{ e.incomingCylinderSizeName }}</td>
              <td>{{ e.outgoingQuantity }}x {{ e.outgoingBrandName }} {{ e.outgoingCylinderSizeName }}</td>
              <td>{{ e.exchangeCharge | number:'1.2-2' }}</td>
              <td>{{ e.notes }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(e.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(e.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="7">No exchanges found.</td></tr>
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
export class CylinderExchangeListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<CylinderExchange[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<CylinderExchange>('cylinderexchanges').subscribe(data => this.items.set(data.items));
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
      this.api.delete('cylinderexchanges', id).subscribe(() => this.loadData());
    }
  }

  onSaved() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.loadData();
  }
}
