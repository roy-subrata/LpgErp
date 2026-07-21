import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

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
      {
        key: 'method',
        label: 'Method',
        kind: 'badge',
        badgeMap: {
          '0': ['#f0fdf4', '#15803d'],
          '1': ['#faf5ff', '#7e22ce'],
          '2': ['#eff6ff', '#1d4ed8'],
          '3': ['#fefce8', '#a16207'],
        },
      },
      {
        key: 'direction',
        label: 'Direction',
        kind: 'badge',
        badgeMap: {
          '0': ['#f0fdf4', '#15803d'],
          '1': ['#fefce8', '#a16207'],
        },
      },
      { key: 'amount', label: 'Amount', kind: 'money' },
      { key: 'paymentDate', label: 'Date', kind: 'date' },
      { key: 'reference', label: 'Reference', kind: 'muted' },
    ],
    fields: [
      { key: 'salesOrderId', label: 'Sales Order', type: 'text' },
      { key: 'purchaseOrderId', label: 'Purchase Order', type: 'text' },
      { key: 'method', label: 'Method', type: 'select', options: [{ label: 'Cash', value: 0 }, { label: 'Bank', value: 1 }, { label: 'Mobile', value: 2 }, { label: 'Cheque', value: 3 }] },
      { key: 'direction', label: 'Direction', type: 'select', options: [{ label: 'Received', value: 0 }, { label: 'Paid', value: 1 }] },
      { key: 'amount', label: 'Amount', type: 'number', required: true },
      { key: 'paymentDate', label: 'Payment Date', type: 'date' },
      { key: 'reference', label: 'Reference', type: 'text' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
    ],
  };
  readonly searchFields = ['salesOrderNumber', 'purchaseOrderNumber', 'reference'];
}
