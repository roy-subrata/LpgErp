import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-cylinder-exchange-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class CylinderExchangeListComponent {
  readonly config: EntityConfig = {
    endpoint: 'cylinderexchanges',
    title: 'Cylinder Exchanges',
    singular: 'Cylinder Exchange',
    cols: [
      { key: 'customerName', label: 'Customer', kind: 'main' },
      { key: 'incomingBrandName', label: 'Incoming Brand', kind: 'text' },
      { key: 'incomingQuantity', label: 'In Qty', kind: 'num' },
      { key: 'outgoingBrandName', label: 'Outgoing Brand', kind: 'text' },
      { key: 'outgoingQuantity', label: 'Out Qty', kind: 'num' },
      { key: 'exchangeCharge', label: 'Charge', kind: 'money' },
      { key: 'exchangeDate', label: 'Date', kind: 'date' },
    ],
    fields: [
      { key: 'customerId', label: 'Customer', type: 'select' },
      { key: 'incomingBrandId', label: 'Incoming Brand', type: 'select' },
      { key: 'incomingCylinderSizeId', label: 'Incoming Cylinder Size', type: 'select' },
      { key: 'incomingQuantity', label: 'Incoming Quantity', type: 'number' },
      { key: 'outgoingBrandId', label: 'Outgoing Brand', type: 'select' },
      { key: 'outgoingCylinderSizeId', label: 'Outgoing Cylinder Size', type: 'select' },
      { key: 'outgoingQuantity', label: 'Outgoing Quantity', type: 'number' },
      { key: 'exchangeCharge', label: 'Exchange Charge', type: 'number' },
      { key: 'exchangeDate', label: 'Exchange Date', type: 'date' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
    ],
  };
  readonly searchFields = ['customerName', 'incomingBrandName', 'outgoingBrandName'];
}
