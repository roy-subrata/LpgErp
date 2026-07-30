import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';
import { PAYMENT_METHOD_OPTIONS, PAYMENT_METHOD_BADGES } from '../../core/payment-methods';

@Component({
  selector: 'app-payment-account-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class PaymentAccountListComponent {
  readonly config: EntityConfig = {
    endpoint: 'paymentaccounts',
    title: 'Payment Accounts',
    singular: 'Payment Account',
    cols: [
      { key: 'name', label: 'Account', kind: 'main' },
      { key: 'method', label: 'Method', kind: 'badge', badgeMap: PAYMENT_METHOD_BADGES },
      { key: 'provider', label: 'Provider' },
      { key: 'accountNumber', label: 'Number', kind: 'mono' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { 'true': ['Active', '#f0fdf4', '#15803d'], 'false': ['Inactive', '#fef2f2', '#b91c1c'] } },
    ],
    fields: [
      { key: 'name', label: 'Account Name', type: 'text', required: true, placeholder: 'bKash Merchant' },
      { key: 'method', label: 'Method', type: 'select', options: PAYMENT_METHOD_OPTIONS, required: true },
      { key: 'provider', label: 'Provider', type: 'text', placeholder: 'bKash / Nagad / City Bank' },
      { key: 'accountNumber', label: 'Wallet or A/C Number', type: 'text', mono: true, placeholder: '01711000001' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
    ],
  };
  readonly searchFields = ['name', 'provider', 'accountNumber'];
}
