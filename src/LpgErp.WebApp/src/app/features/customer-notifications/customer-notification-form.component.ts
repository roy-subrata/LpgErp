import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Customer, CustomerNotification } from '../../core/models';

@Component({
  selector: 'app-customer-notification-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Notification' : 'New Notification'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="customerId">Customer</label>
          <select id="customerId" [(ngModel)]="customerId" name="customerId" required>
            <option value="">Select Customer</option>
            @for (customer of customers(); track customer.id) {
              <option [value]="customer.id">{{ customer.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="type">Type</label>
          <select id="type" [(ngModel)]="type" name="type" required>
            <option [ngValue]="0">Payment Due</option>
            <option [ngValue]="1">Possible Refill Time</option>
            <option [ngValue]="2">Cylinder Return Reminder</option>
            <option [ngValue]="3">Outstanding Empty Cylinder</option>
            <option [ngValue]="4">Credit Limit Exceeded</option>
          </select>
        </div>
        <div class="form-group">
          <label for="title">Title</label>
          <input id="title" type="text" [(ngModel)]="title" name="title" required />
        </div>
        <div class="form-group">
          <label for="message">Message</label>
          <input id="message" type="text" [(ngModel)]="message" name="message" required />
        </div>
        <div class="form-group">
          <label>
            <input type="checkbox" [(ngModel)]="isRead" name="isRead" />
            Read
          </label>
        </div>
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="onClose()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">Save</button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group input, .form-group select { width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class CustomerNotificationFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  customerId = '';
  type = 0;
  title = '';
  message = '';
  isRead = false;
  customers = signal<Customer[]>([]);
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.api.getAllList<Customer>('customers').subscribe(data => this.customers.set(data));
      if (this.entityId) {
        this.api.getById<CustomerNotification>('customernotifications', this.entityId).subscribe(notification => {
          this.customerId = notification.customerId;
          this.type = notification.type;
          this.title = notification.title;
          this.message = notification.message;
          this.isRead = notification.isRead;
        });
      } else {
        this.resetForm();
      }
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      customerId: this.customerId,
      type: Number(this.type),
      title: this.title,
      message: this.message,
    };

    const req$ = this.entityId
      ? this.api.update('customernotifications', this.entityId, body)
      : this.api.create('customernotifications', body);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.emit();
        this.resetForm();
      },
      error: () => this.saving.set(false),
    });
  }

  onClose() {
    this.resetForm();
    this.close.emit();
  }

  private resetForm() {
    this.customerId = '';
    this.type = 0;
    this.title = '';
    this.message = '';
    this.isRead = false;
  }
}
