import { Component, EventEmitter, Input, Output, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../core/api.service';
import { PaymentAccount } from '../core/models';
import { PAYMENT_METHOD_OPTIONS, requiresAccount } from '../core/payment-methods';

/**
 * "How did the money move" — the method and the specific wallet or bank account, kept together
 * because one is meaningless without the other. Used anywhere cash changes hands.
 */
@Component({
  selector: 'app-payment-method-picker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="form-group">
      <label [attr.for]="idPrefix + '-method'">{{ methodLabel }}</label>
      <select [attr.id]="idPrefix + '-method'" [ngModel]="method" [name]="idPrefix + '-method'"
              (ngModelChange)="onMethodChange($event)">
        @for (m of methodOptions; track m.value) {
          <option [ngValue]="m.value">{{ m.label }}</option>
        }
      </select>
    </div>
    <div class="form-group">
      <label [attr.for]="idPrefix + '-account'">Account</label>
      <select [attr.id]="idPrefix + '-account'" [ngModel]="accountId" [name]="idPrefix + '-account'"
              (ngModelChange)="accountIdChange.emit($event)">
        <option value="">{{ required() ? '-- Select --' : '-- None (cash in hand) --' }}</option>
        @for (a of accountsForMethod(); track a.id) {
          <option [value]="a.id">{{ a.name }}{{ a.accountNumber ? ' · ' + a.accountNumber : '' }}</option>
        }
      </select>
      @if (required() && accountsForMethod().length === 0) {
        <small class="hint warn">No active account for this method — add one under Transactions → Payment Accounts.</small>
      } @else {
        <small class="hint">Which wallet or bank account the money moved through.</small>
      }
    </div>
  `,
  styles: [`
    .hint { display: block; margin-top: 0.25rem; font-size: 0.75rem; color: #6b7280; }
    .hint.warn { color: #b45309; }
  `],
})
export class PaymentMethodPickerComponent implements OnInit {
  private api = inject(ApiService);

  @Input() method = 0;
  @Output() methodChange = new EventEmitter<number>();

  @Input() accountId = '';
  @Output() accountIdChange = new EventEmitter<string>();

  /** Distinguishes this picker's control names when a form hosts more than one. */
  @Input() idPrefix = 'pay';
  @Input() methodLabel = 'Paid By';

  readonly methodOptions = PAYMENT_METHOD_OPTIONS;
  accounts = signal<PaymentAccount[]>([]);

  ngOnInit() {
    this.api.getAllList<PaymentAccount>('paymentaccounts').subscribe(data => this.accounts.set(data));
  }

  /** Only accounts of the selected method — a bKash payment cannot land in a bank account. */
  accountsForMethod(): PaymentAccount[] {
    return this.accounts().filter(a => a.method === Number(this.method));
  }

  required(): boolean {
    return requiresAccount(Number(this.method));
  }

  onMethodChange(value: number) {
    this.method = value;
    this.methodChange.emit(value);

    // The chosen account belongs to the old method; clear it rather than send a mismatch.
    if (!this.accountsForMethod().some(a => a.id === this.accountId)) {
      this.accountId = '';
      this.accountIdChange.emit('');
    }
  }
}
