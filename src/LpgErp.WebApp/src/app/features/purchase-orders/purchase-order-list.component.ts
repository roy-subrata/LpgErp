import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { PurchaseOrder } from '../../core/models';
import { PurchaseOrderFormComponent } from './purchase-order-form.component';

@Component({
  selector: 'app-purchase-order-list',
  standalone: true,
  imports: [CommonModule, PurchaseOrderFormComponent],
  template: `
    <div class="page-header">
      <h1>Purchase Orders</h1>
      <button class="btn-primary" (click)="onNew()">+ New Purchase Order</button>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Order Number</th>
            <th>Supplier</th>
            <th>Warehouse</th>
            <th>Date</th>
            <th>Total Amount</th>
            <th>Commission</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (o of items(); track o.id) {
            <tr>
              <td>{{ o.orderNumber }}</td>
              <td>{{ o.supplierName }}</td>
              <td>{{ o.warehouseName }}</td>
              <td>{{ o.orderDate | date:'shortDate' }}</td>
              <td>{{ o.totalAmount | number:'1.2-2' }}</td>
              <td>{{ o.commissionEarned | number:'1.2-2' }}</td>
              <td>{{ orderStatus(o.status) }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(o.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(o.id)">Delete</button>
                @if (o.status === 0) {
                  <button class="btn-sm btn-success" (click)="onConfirm(o.id)">Confirm</button>
                }
                @if (o.status === 1) {
                  <button class="btn-sm btn-success" (click)="onReceive(o.id)">Receive</button>
                }
              </td>
            </tr>
          } @empty {
            <tr><td colspan="8">No purchase orders found.</td></tr>
          }
        </tbody>
      </table>
    </div>
    <app-purchase-order-form [open]="showForm()" [entityId]="editingId()" (close)="showForm.set(false)" (saved)="onSaved()" />
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
export class PurchaseOrderListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<PurchaseOrder[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<PurchaseOrder>('purchaseorders').subscribe(data => this.items.set(data.items));
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

  onConfirm(id: string) {
    if (confirm('Confirm this purchase order?')) {
      this.api.post('purchaseorders/' + id + '/confirm', {}).subscribe(() => this.loadData());
    }
  }

  onReceive(id: string) {
    if (confirm('Mark this purchase order as received?')) {
      this.api.post('purchaseorders/' + id + '/receive', {}).subscribe(() => this.loadData());
    }
  }

  onDelete(id: string) {
    if (confirm('Are you sure you want to delete this purchase order?')) {
      this.api.delete('purchaseorders', id).subscribe(() => this.loadData());
    }
  }

  orderStatus(status: number): string {
    const map: Record<number, string> = { 0: 'Draft', 1: 'Confirmed', 2: 'PartiallyReceived', 3: 'Received', 4: 'Cancelled' };
    return map[status] ?? 'Unknown';
  }
}
