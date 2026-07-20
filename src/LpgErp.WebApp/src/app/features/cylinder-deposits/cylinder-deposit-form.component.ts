import { Component, EventEmitter, Input, Output, inject, signal, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Customer, CylinderSize, CylinderDeposit } from '../../core/models';

@Component({
  selector: 'app-cylinder-deposit-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Cylinder Deposit' : 'New Cylinder Deposit'" (close)="onClose()">
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
          <label for="cylinderSizeId">Cylinder Size</label>
          <select id="cylinderSizeId" [(ngModel)]="cylinderSizeId" name="cylinderSizeId" required>
            <option value="">Select Cylinder Size</option>
            @for (size of cylinderSizes(); track size.id) {
              <option [value]="size.id">{{ size.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="type">Type</label>
          <select id="type" [(ngModel)]="type" name="type" required>
            <option [ngValue]="0">Paid</option>
            <option [ngValue]="1">Returned</option>
            <option [ngValue]="2">Refund</option>
          </select>
        </div>
        <div class="form-group">
          <label for="amount">Amount</label>
          <input id="amount" type="number" [(ngModel)]="amount" name="amount" required />
        </div>
        <div class="form-group">
          <label for="quantity">Quantity</label>
          <input id="quantity" type="number" [(ngModel)]="quantity" name="quantity" required />
        </div>
        <div class="form-group">
          <label for="reference">Reference</label>
          <input id="reference" type="text" [(ngModel)]="reference" name="reference" />
        </div>
        <div class="form-group">
          <label for="notes">Notes</label>
          <input id="notes" type="text" [(ngModel)]="notes" name="notes" />
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
export class CylinderDepositFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  customerId = '';
  cylinderSizeId = '';
  type = 0;
  amount = 0;
  quantity = 0;
  reference = '';
  notes = '';
  customers = signal<Customer[]>([]);
  cylinderSizes = signal<CylinderSize[]>([]);
  saving = signal(false);

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.api.getAllList<Customer>('customers').subscribe(data => this.customers.set(data));
      this.api.getAllList<CylinderSize>('cylindersizes').subscribe(data => this.cylinderSizes.set(data));
      if (this.entityId) {
        this.api.getById<CylinderDeposit>('cylinderdeposits', this.entityId).subscribe(deposit => {
          this.customerId = deposit.customerId;
          this.cylinderSizeId = deposit.cylinderSizeId;
          this.type = deposit.type;
          this.amount = deposit.amount;
          this.quantity = deposit.quantity;
          this.reference = deposit.reference;
          this.notes = deposit.notes;
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
      cylinderSizeId: this.cylinderSizeId,
      type: Number(this.type),
      amount: this.amount,
      quantity: this.quantity,
      reference: this.reference,
      notes: this.notes,
    };

    const req$ = this.entityId
      ? this.api.update('cylinderdeposits', this.entityId, body)
      : this.api.create('cylinderdeposits', body);

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
    this.cylinderSizeId = '';
    this.type = 0;
    this.amount = 0;
    this.quantity = 0;
    this.reference = '';
    this.notes = '';
  }
}
