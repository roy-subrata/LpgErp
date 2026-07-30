import { Component, EventEmitter, Input, Output, inject, signal, computed, SimpleChanges, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogComponent } from '../../shared/dialog.component';
import { ApiService } from '../../core/api.service';
import { Payment, SalesOrder, PurchaseOrder, PaymentAccount } from '../../core/models';
import { PAYMENT_METHOD_OPTIONS, requiresAccount } from '../../core/payment-methods';

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent],
  template: `
    <app-dialog [open]="open" [title]="entityId ? 'Edit Payment' : 'New Payment'" (close)="onClose()">
      <form (ngSubmit)="submit()">
        <p class="form-hint">Link the payment to either a sales order or a purchase order — a payment
          attached to neither cannot be attributed to a customer or supplier.</p>
        <div class="form-group">
          <label for="salesOrderId">Sales Order</label>
          <select id="salesOrderId" [(ngModel)]="salesOrderId" name="salesOrderId">
            <option value="">-- None --</option>
            @for (so of salesOrders(); track so.id) {
              <option [value]="so.id">{{ so.orderNumber }} - {{ so.customerName }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="purchaseOrderId">Purchase Order</label>
          <select id="purchaseOrderId" [(ngModel)]="purchaseOrderId" name="purchaseOrderId">
            <option value="">-- None --</option>
            @for (po of purchaseOrders(); track po.id) {
              <option [value]="po.id">{{ po.orderNumber }} - {{ po.supplierName }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="method">Method</label>
          <select id="method" [(ngModel)]="method" name="method" required (ngModelChange)="onMethodChange()">
            @for (m of methodOptions; track m.value) {
              <option [ngValue]="m.value">{{ m.label }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label for="paymentAccountId">Account</label>
          <select id="paymentAccountId" [(ngModel)]="paymentAccountId" name="paymentAccountId" [required]="accountRequired()">
            <option value="">{{ accountRequired() ? '-- Select --' : '-- None (cash in hand) --' }}</option>
            @for (a of accountsForMethod(); track a.id) {
              <option [value]="a.id">{{ a.name }}{{ a.accountNumber ? ' · ' + a.accountNumber : '' }}</option>
            }
          </select>
          @if (accountRequired() && accountsForMethod().length === 0) {
            <small class="field-hint">No active account for this method yet — add one under Master Data → Payment Accounts.</small>
          } @else {
            <small class="field-hint">Which wallet or bank account the money moved through.</small>
          }
        </div>
        <div class="form-group">
          <label for="direction">Direction</label>
          <select id="direction" [(ngModel)]="direction" name="direction" required>
            <option [ngValue]="0">Inbound (Receive)</option>
            <option [ngValue]="1">Outbound (Pay)</option>
          </select>
        </div>
        <div class="form-group">
          <label for="amount">Amount</label>
          <input id="amount" type="number" [(ngModel)]="amount" name="amount" required step="0.01" />
        </div>
        <div class="form-group">
          <label for="paymentDate">Payment Date</label>
          <input id="paymentDate" type="date" [(ngModel)]="paymentDate" name="paymentDate" required />
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
          <button type="submit" class="btn btn-primary" [disabled]="saving()">{{ saving() ? 'Saving...' : (entityId ? 'Update' : 'Create') }}</button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .form-hint { margin: 0 0 1rem; color: #666; font-size: 0.85rem; line-height: 1.4; }
    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group input, .form-group select { width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class PaymentFormComponent implements OnChanges {
  private api = inject(ApiService);

  @Input() open = false;
  @Input() entityId: string | null = null;

  /** When set, only this customer's orders are offered — used from the customer account page. */
  @Input() customerId: string | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  salesOrderId = '';
  purchaseOrderId = '';
  method = 0;
  paymentAccountId = '';
  direction = 0;
  amount = 0;
  paymentDate = '';
  reference = '';
  notes = '';
  saving = signal(false);

  salesOrders = signal<SalesOrder[]>([]);
  purchaseOrders = signal<PurchaseOrder[]>([]);
  accounts = signal<PaymentAccount[]>([]);

  readonly methodOptions = PAYMENT_METHOD_OPTIONS;

  /** Only accounts of the selected method — a bKash payment can't land in a bank account. */
  accountsForMethod = computed(() => this.accounts().filter(a => a.method === Number(this.method)));
  accountRequired = () => requiresAccount(Number(this.method));

  onMethodChange() {
    // The previously picked account belongs to the old method; drop it rather than send a mismatch.
    if (!this.accountsForMethod().some(a => a.id === this.paymentAccountId)) {
      this.paymentAccountId = '';
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open) {
      this.loadDropdownData();
      this.api.getAllList<PaymentAccount>('paymentaccounts').subscribe(data => this.accounts.set(data));
      if (this.entityId) {
        this.api.getById<Payment>('payments', this.entityId).subscribe(payment => {
          this.salesOrderId = payment.salesOrderId;
          this.purchaseOrderId = payment.purchaseOrderId;
          this.method = payment.method;
          this.paymentAccountId = payment.paymentAccountId ?? '';
          this.direction = payment.direction;
          this.amount = payment.amount;
          this.paymentDate = payment.paymentDate?.split('T')[0] || '';
          this.reference = payment.reference;
          this.notes = payment.notes;
        });
      } else {
        this.resetForm();
      }
    }
  }

  submit() {
    this.saving.set(true);
    const body = {
      salesOrderId: this.salesOrderId || null,
      purchaseOrderId: this.purchaseOrderId || null,
      method: Number(this.method),
      paymentAccountId: this.paymentAccountId || null,
      direction: Number(this.direction),
      amount: this.amount,
      paymentDate: this.paymentDate,
      reference: this.reference,
      notes: this.notes,
    };

    const req$ = this.entityId
      ? this.api.update('payments', this.entityId, body)
      : this.api.create('payments', body);

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

  private loadDropdownData() {
    this.api.getAll<SalesOrder>('salesorders', 1, 1000).subscribe(data =>
      this.salesOrders.set(this.customerId
        ? data.items.filter(so => (so as any).customerId === this.customerId)
        : data.items));
    this.api.getAll<PurchaseOrder>('purchaseorders', 1, 1000).subscribe(data => this.purchaseOrders.set(data.items));
  }

  private resetForm() {
    this.salesOrderId = '';
    this.purchaseOrderId = '';
    this.method = 0;
    this.paymentAccountId = '';
    this.direction = 0;
    this.amount = 0;
    this.paymentDate = '';
    this.reference = '';
    this.notes = '';
  }
}
