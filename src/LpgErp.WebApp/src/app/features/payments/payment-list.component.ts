import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';
import { PAYMENT_METHOD_OPTIONS, PAYMENT_METHOD_BADGES, PAYMENT_DIRECTION_BADGES } from '../../core/payment-methods';

@Component({
  selector: 'app-payment-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class PaymentListComponent {
  readonly config: EntityConfig = {
    endpoint: 'payments',
    title: 'Payments',
    singular: 'Payment',
    cols: [
      { key: 'salesOrderNumber', label: 'Sales Order', kind: 'mono', sub: 'purchaseOrderNumber' },
      { key: 'method', label: 'Method', kind: 'badge', badgeMap: PAYMENT_METHOD_BADGES },
      { key: 'paymentAccountName', label: 'Account', kind: 'muted' },
      { key: 'direction', label: 'Direction', kind: 'badge', badgeMap: PAYMENT_DIRECTION_BADGES },
      { key: 'amount', label: 'Amount', kind: 'money' },
      { key: 'paymentDate', label: 'Date', kind: 'date' },
      { key: 'reference', label: 'Reference', kind: 'muted' },
    ],
    fields: [
      { key: 'salesOrderId', label: 'Sales Order', type: 'text' },
      { key: 'purchaseOrderId', label: 'Purchase Order', type: 'text' },
      { key: 'method', label: 'Method', type: 'select', options: PAYMENT_METHOD_OPTIONS },
      { key: 'paymentAccountId', label: 'Account', type: 'select', endpoint: 'paymentaccounts', optionLabel: 'name' },
      { key: 'direction', label: 'Direction', type: 'select', options: [{ label: 'Received', value: 0 }, { label: 'Paid', value: 1 }] },
      { key: 'amount', label: 'Amount', type: 'number', required: true },
      { key: 'paymentDate', label: 'Payment Date', type: 'date' },
      { key: 'reference', label: 'Reference', type: 'text' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
    ],
  };
  readonly searchFields = ['salesOrderNumber', 'purchaseOrderNumber', 'reference', 'paymentAccountName'];
}
