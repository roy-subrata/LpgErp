import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-stock-transfer-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class StockTransferListComponent {
  readonly config: EntityConfig = {
    endpoint: 'stockmovements',
    title: 'Stock Transfers',
    singular: 'Stock Transfer',
    cols: [
      { key: 'productName', label: 'Product', kind: 'main' },
      { key: 'fromWarehouseName', label: 'From', kind: 'text' },
      { key: 'toWarehouseName', label: 'To', kind: 'text' },
      { key: 'quantity', label: 'Quantity', kind: 'num' },
      { key: 'reference', label: 'Reference', kind: 'mono' },
      { key: 'movementDate', label: 'Date', kind: 'date' },
    ],
    fields: [
      { key: 'productId', label: 'Product', type: 'select', endpoint: 'products', optionLabel: 'name' },
      { key: 'fromWarehouseId', label: 'From Warehouse', type: 'select', endpoint: 'warehouses', optionLabel: 'name' },
      { key: 'toWarehouseId', label: 'To Warehouse', type: 'select', endpoint: 'warehouses', optionLabel: 'name' },
      { key: 'quantity', label: 'Quantity', type: 'number', required: true },
      { key: 'reference', label: 'Reference', type: 'text' },
    ],
  };
  readonly searchFields = ['productName', 'fromWarehouseName', 'toWarehouseName', 'reference'];
}
