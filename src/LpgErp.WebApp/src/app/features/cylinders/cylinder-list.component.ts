import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-cylinder-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class CylinderListComponent {
  readonly config: EntityConfig = {
    endpoint: 'cylinders',
    title: 'Cylinders',
    singular: 'Cylinder',
    cols: [
      { key: 'serialNumber', label: 'Serial Number', kind: 'mono' },
      { key: 'brandName', label: 'Brand', kind: 'main' },
      { key: 'cylinderSizeName', label: 'Size', kind: 'text' },
      { key: 'currentWarehouseName', label: 'Warehouse', kind: 'text' },
      { key: 'status', label: 'Status', kind: 'badge', badgeMap: { '0': ['Available', '#f0fdf4', '#15803d'], '1': ['On Load', '#eff6ff', '#1d4ed8'], '2': ['With Customer', '#fefce8', '#a16207'], '3': ['Damaged', '#fef2f2', '#dc2626'] } },
      { key: 'hasGas', label: 'Gas', kind: 'badge', badgeMap: { true: ['Filled', '#f0fdf4', '#15803d'], false: ['Empty', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'brandId', label: 'Brand', type: 'select', endpoint: 'brands', optionLabel: 'name' },
      { key: 'cylinderSizeId', label: 'Cylinder Size', type: 'select', endpoint: 'cylindersizes', optionLabel: 'name' },
      { key: 'currentWarehouseId', label: 'Warehouse', type: 'select', endpoint: 'warehouses', optionLabel: 'name' },
      { key: 'serialNumber', label: 'Serial Number', type: 'text', required: true, mono: true },
      { key: 'status', label: 'Status', type: 'select', options: [{ value: 0, label: 'Available' }, { value: 1, label: 'On Load' }, { value: 2, label: 'With Customer' }, { value: 3, label: 'Damaged' }] },
      { key: 'hasGas', label: 'Has Gas', type: 'toggle' },
    ],
  };
  readonly searchFields = ['serialNumber', 'brandName', 'cylinderSizeName', 'currentWarehouseName'];
}
