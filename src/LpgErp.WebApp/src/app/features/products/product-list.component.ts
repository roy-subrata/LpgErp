import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class ProductListComponent {
  readonly config: EntityConfig = {
    endpoint: 'products',
    title: 'Products',
    singular: 'Product',
    cols: [
      { key: 'name', label: 'Name', kind: 'main', sub: 'brandName' },
      { key: 'code', label: 'Code', kind: 'mono' },
      { key: 'type', label: 'Type', kind: 'badge', badgeMap: { '0': ['Empty Cylinder', '#f4f5f7', '#6b7280'], '1': ['Gas Refill', '#f0fdf4', '#15803d'], '2': ['New Package', '#eff6ff', '#1d4ed8'], '3': ['Accessory', '#faf5ff', '#7e22ce'] } },
      { key: 'purchasePrice', label: 'Purchase Price', kind: 'money' },
      { key: 'salePrice', label: 'Sale Price', kind: 'money' },
      { key: 'currentStock', label: 'Stock', kind: 'num' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'code', label: 'Code', type: 'text', required: true, mono: true },
      { key: 'type', label: 'Type', type: 'select', options: [{ value: 0, label: 'Empty Cylinder' }, { value: 1, label: 'Gas Refill' }, { value: 2, label: 'New Package' }, { value: 3, label: 'Accessory' }] },
      { key: 'purchasePrice', label: 'Purchase Price', type: 'number' },
      { key: 'salePrice', label: 'Sale Price', type: 'number' },
      { key: 'currentStock', label: 'Current Stock', type: 'number' },
      { key: 'minimumStock', label: 'Minimum Stock', type: 'number' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ],
  };
  readonly searchFields = ['name', 'code'];
}
