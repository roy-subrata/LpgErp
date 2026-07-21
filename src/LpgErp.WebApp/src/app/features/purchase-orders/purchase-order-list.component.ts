import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-purchase-order-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class PurchaseOrderListComponent {
  readonly config: EntityConfig = {
    endpoint: 'purchaseorders',
    title: 'Purchase Orders',
    singular: 'Purchase Order',
    cols: [
      { key: 'orderNumber', label: 'Order Number', kind: 'mono', width: '130px' },
      { key: 'supplierName', label: 'Supplier', kind: 'main' },
      { key: 'warehouseName', label: 'Warehouse', kind: 'text' },
      { key: 'orderDate', label: 'Order Date', kind: 'date' },
      { key: 'totalAmount', label: 'Total Amount', kind: 'money' },
      {
        key: 'status',
        label: 'Status',
        kind: 'badge',
        badgeMap: {
          'Draft': ['#f4f5f7', '#6b7280'],
          'Confirmed': ['#dbeafe', '#1d4ed8'],
          'Partially Received': ['#fef3c7', '#92400e'],
          'Received': ['#dcfce7', '#166534'],
          'Cancelled': ['#fee2e2', '#991b1b'],
        },
      },
    ],
    fields: [
      { key: 'supplierId', label: 'Supplier', type: 'select', endpoint: 'suppliers', optionLabel: 'name' },
      { key: 'warehouseId', label: 'Warehouse', type: 'select', endpoint: 'warehouses', optionLabel: 'name' },
      { key: 'orderDate', label: 'Order Date', type: 'date' },
      { key: 'expectedDeliveryDate', label: 'Expected Delivery Date', type: 'date' },
      { key: 'notes', label: 'Notes', type: 'textarea' },
    ],
  };
  readonly searchFields = ['orderNumber', 'supplierName', 'warehouseName'];
}
