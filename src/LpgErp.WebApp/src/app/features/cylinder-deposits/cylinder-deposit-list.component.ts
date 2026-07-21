import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-cylinder-deposit-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class CylinderDepositListComponent {
  readonly config: EntityConfig = {
    endpoint: 'cylinderdeposits',
    title: 'Cylinder Deposits',
    singular: 'Cylinder Deposit',
    cols: [
      { key: 'customerName', label: 'Customer', kind: 'main' },
      { key: 'cylinderSizeName', label: 'Cylinder Size', kind: 'text' },
      {
        key: 'type',
        label: 'Type',
        kind: 'badge',
        badgeMap: {
          '0': ['#dbeafe', '#1d4ed8'],
          '1': ['#fef3c7', '#92400e'],
        },
      },
      { key: 'amount', label: 'Amount', kind: 'money' },
      { key: 'quantity', label: 'Quantity', kind: 'num' },
      { key: 'depositDate', label: 'Date', kind: 'date' },
      { key: 'reference', label: 'Reference', kind: 'mono' },
    ],
    fields: [
      { key: 'customerId', label: 'Customer', type: 'select' },
      { key: 'cylinderSizeId', label: 'Cylinder Size', type: 'select' },
      { key: 'type', label: 'Type', type: 'select', options: [{ label: 'Deposit', value: 0 }, { label: 'Refund', value: 1 }] },
      { key: 'amount', label: 'Amount', type: 'number' },
      { key: 'quantity', label: 'Quantity', type: 'number' },
      { key: 'depositDate', label: 'Deposit Date', type: 'date' },
      { key: 'reference', label: 'Reference', type: 'text' },
      { key: 'notes', label: 'Notes', type: 'text' },
    ],
  };
  readonly searchFields = ['customerName', 'cylinderSizeName', 'reference'];
}
