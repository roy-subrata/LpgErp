import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { Payment } from '../../core/models';
import { PaymentFormComponent } from './payment-form.component';

@Component({
  selector: 'app-payment-list',
  standalone: true,
  imports: [CommonModule, PaymentFormComponent],
  template: `
    <div class="page-header">
      <h1>Payments</h1>
      <button class="btn-primary" (click)="onNew()">+ New Payment</button>
    </div>
    <app-payment-form [open]="showForm()" [entityId]="editingId()" (saved)="onSaved()" (close)="showForm.set(false)" />
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Sales Order</th>
            <th>Purchase Order</th>
            <th>Method</th>
            <th>Direction</th>
            <th>Amount</th>
            <th>Reference</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (p of items(); track p.id) {
            <tr>
              <td>{{ p.paymentDate | date:'shortDate' }}</td>
              <td>{{ p.salesOrderNumber || '-' }}</td>
              <td>{{ p.purchaseOrderNumber || '-' }}</td>
              <td>{{ paymentMethod(p.method) }}</td>
              <td>{{ p.direction === 1 ? 'Inbound' : 'Outbound' }}</td>
              <td>{{ p.amount | number:'1.2-2' }}</td>
              <td>{{ p.reference }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(p.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(p.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="8">No payments found.</td></tr>
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
export class PaymentListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<Payment[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<Payment>('payments').subscribe(data => this.items.set(data.items));
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
      this.api.delete('payments', id).subscribe(() => this.loadData());
    }
  }

  onSaved() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.loadData();
  }

  paymentMethod(method: number): string {
    const map: Record<number, string> = { 0: 'Cash', 1: 'Credit', 2: 'Bank Transfer', 3: 'Mobile Banking' };
    return map[method] ?? 'Unknown';
  }
}
