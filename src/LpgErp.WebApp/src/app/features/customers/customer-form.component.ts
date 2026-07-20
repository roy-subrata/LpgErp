import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Customer } from '../../core/models';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Customer' : 'New Customer'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label for="name">Name</label>
          <input id="name" type="text" [(ngModel)]="name" name="name" required />
        </div>
        <div class="form-group">
          <label for="code">Code</label>
          <input id="code" type="text" [(ngModel)]="code" name="code" required />
        </div>
        <div class="form-group">
          <label for="type">Type</label>
          <select id="type" [(ngModel)]="type" name="type">
            <option [ngValue]="0">Retail</option>
            <option [ngValue]="1">Wholesale</option>
            <option [ngValue]="2">Commercial</option>
            <option [ngValue]="3">Restaurant</option>
            <option [ngValue]="4">Hotel</option>
            <option [ngValue]="5">Industrial</option>
          </select>
        </div>
        <div class="form-group">
          <label for="contactPerson">Contact Person</label>
          <input id="contactPerson" type="text" [(ngModel)]="contactPerson" name="contactPerson" />
        </div>
        <div class="form-group">
          <label for="phone">Phone</label>
          <input id="phone" type="text" [(ngModel)]="phone" name="phone" />
        </div>
        <div class="form-group">
          <label for="email">Email</label>
          <input id="email" type="email" [(ngModel)]="email" name="email" />
        </div>
        <div class="form-group">
          <label for="address">Address</label>
          <input id="address" type="text" [(ngModel)]="address" name="address" />
        </div>
        <div class="form-group">
          <label for="creditLimit">Credit Limit</label>
          <input id="creditLimit" type="number" [(ngModel)]="creditLimit" name="creditLimit" />
        </div>
        <div class="form-group">
          <label for="paymentDueDays">Payment Due Days</label>
          <input id="paymentDueDays" type="number" [(ngModel)]="paymentDueDays" name="paymentDueDays" />
        </div>
        <div class="form-group">
          <label>
            <input type="checkbox" [(ngModel)]="isActive" name="isActive" />
            Active
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
    .form-group input[type="checkbox"] { width: auto; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class CustomerFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  name = '';
  code = '';
  type = 0;
  contactPerson = '';
  phone = '';
  email = '';
  address = '';
  creditLimit = 0;
  paymentDueDays = 30;
  isActive = true;
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      if (this.entityId) {
        this.api.getById<Customer>('customers', this.entityId).subscribe(customer => {
          this.name = customer.name;
          this.code = customer.code;
          this.type = customer.type;
          this.contactPerson = customer.contactPerson;
          this.phone = customer.phone;
          this.email = customer.email;
          this.address = customer.address;
          this.creditLimit = customer.creditLimit;
          this.paymentDueDays = customer.paymentDueDays;
          this.isActive = customer.isActive;
        });
      } else {
        this.resetForm();
      }
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      name: this.name,
      code: this.code,
      type: Number(this.type),
      contactPerson: this.contactPerson,
      phone: this.phone,
      email: this.email,
      address: this.address,
      creditLimit: this.creditLimit,
      paymentDueDays: this.paymentDueDays,
      isActive: this.isActive,
    };

    const req$ = this.entityId
      ? this.api.update('customers', this.entityId, body)
      : this.api.create('customers', body);

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
    this.name = '';
    this.code = '';
    this.type = 0;
    this.contactPerson = '';
    this.phone = '';
    this.email = '';
    this.address = '';
    this.creditLimit = 0;
    this.paymentDueDays = 30;
    this.isActive = true;
  }
}
