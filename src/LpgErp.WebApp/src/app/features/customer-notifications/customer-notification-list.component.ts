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
      <div class="header-actions">
        <button class="btn-secondary" [disabled]="generating()" (click)="onGenerate()">
          {{ generating() ? 'Scanning…' : '⚡ Generate' }}
        </button>
        <button class="btn-primary" (click)="onNew()">+ New Notification</button>
      </div>
    </div>
    @if (generateResult()) {
      <div class="generate-banner">{{ generateResult() }}</div>
    }
    <app-customer-notification-form [open]="showForm()" [entityId]="editingId()" (saved)="onSaved()" (close)="showForm.set(false)" />
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
    .header-actions { display: flex; gap: 0.5rem; }
    .btn-primary { background: #1a1a2e; color: white; border: none; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; }
    .btn-secondary { background: white; color: #1a1a2e; border: 1px solid #1a1a2e; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; }
    .btn-secondary:disabled { opacity: 0.6; cursor: default; }
    .generate-banner { background: #f0fdf4; border: 1px solid #bbf7d0; color: #15803d; padding: 0.6rem 1rem; border-radius: 6px; margin-bottom: 1rem; font-size: 0.9rem; }
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
  generating = signal(false);
  generateResult = signal('');

  onGenerate() {
    this.generating.set(true);
    this.generateResult.set('');
    this.api.post<number>('customernotifications/generate', {}).subscribe({
      next: (count) => {
        this.generating.set(false);
        this.generateResult.set(count > 0 ? `${count} notification(s) generated.` : 'No new notifications — everything is up to date.');
        this.loadData();
      },
      error: () => {
        this.generating.set(false);
        this.generateResult.set('Generation failed.');
      },
    });
  }

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
    // Mirrors backend NotificationType enum.
    const map: Record<number, string> = {
      0: 'Payment Due',
      1: 'Possible Refill Time',
      2: 'Cylinder Return Reminder',
      3: 'Outstanding Empty Cylinder',
      4: 'Credit Limit Exceeded',
    };
    return map[type] ?? 'Unknown';
  }
}
