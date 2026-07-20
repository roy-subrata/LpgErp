import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { CustomerNotification } from '../../core/models';
import { CustomerNotificationFormComponent } from './customer-notification-form.component';

@Component({
  selector: 'app-customer-notification-list',
  standalone: true,
  imports: [CommonModule, CustomerNotificationFormComponent],
  template: `
    <div class="page-header">
      <h1>Customer Notifications</h1>
      <button class="btn-primary" (click)="onNew()">+ New Notification</button>
    </div>
    @if (showForm()) {
      <app-customer-notification-form [entityId]="editingId()" (saved)="onSaved()" (close)="showForm.set(false)" />
    }
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Customer</th>
            <th>Type</th>
            <th>Title</th>
            <th>Message</th>
            <th>Read</th>
            <th>Sent</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (n of items(); track n.id) {
            <tr>
              <td>{{ n.customerName }}</td>
              <td>{{ notificationType(n.type) }}</td>
              <td>{{ n.title }}</td>
              <td>{{ n.message }}</td>
              <td>{{ n.isRead ? 'Yes' : 'No' }}</td>
              <td>{{ n.isSent ? 'Yes' : 'No' }}</td>
              <td>
                <button class="btn-sm" (click)="onEdit(n.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(n.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="7">No notifications found.</td></tr>
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
export class CustomerNotificationListComponent implements OnInit {
  private api = inject(ApiService);
  items = signal<CustomerNotification[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAll<CustomerNotification>('customernotifications').subscribe(data => this.items.set(data.items));
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
      this.api.delete('customernotifications', id).subscribe(() => this.loadData());
    }
  }

  onSaved() {
    this.showForm.set(false);
    this.editingId.set(null);
    this.loadData();
  }

  notificationType(type: number): string {
    const map: Record<number, string> = { 0: 'LowStock', 1: 'PriceChange', 2: 'DeliveryUpdate', 3: 'PaymentReminder', 4: 'General' };
    return map[type] ?? 'Unknown';
  }
}
