import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { CylinderDeposit } from '../../core/models';
import { CylinderDepositFormComponent } from './cylinder-deposit-form.component';

@Component({
  selector: 'app-cylinder-deposit-list',
  standalone: true,
  imports: [CommonModule, CylinderDepositFormComponent],
  template: `
    <div class="page-header">
      <h1>Cylinder Deposits</h1>
      <button class="btn-primary" (click)="onNew()">+ New Deposit</button>
    </div>
    <app-cylinder-deposit-form [open]="showForm()" [entityId]="editingId()" (saved)="onSaved()" (close)="showForm.set(false)" />
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Customer</th>
            <th>Cylinder Size</th>
            <th>Type</th>
            <th>Amount</th>
            <th>Quantity</th>
            <th>Reference</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (d of items(); track d.id) {
            <tr>
              <td>{{ d.depositDate | date:'short' }}</td>
              <td>{{ d.customerName }}</td>
              <td>{{ d.cylinderSizeName }}</td>
              <td>{{ d.type === 0 ? 'Deposit' : 'Refund' }}</td>
              <td>{{ d.amount | number:'1.2-2' }}</td>
              <td>{{ d.quantity }}</td>
              <td>{{ d.reference }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(d.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(d.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="8">No deposits found.</td></tr>
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
export class CylinderDepositListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<CylinderDeposit[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<CylinderDeposit>('cylinderdeposits').subscribe(data => this.items.set(data.items));
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
      this.api.delete('cylinderdeposits', id).subscribe(() => this.loadData());
    }
  }

  onSaved() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.loadData();
  }
}
