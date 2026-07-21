import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-warehouse-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class WarehouseListComponent {
  readonly config: EntityConfig = {
    endpoint: 'warehouses',
    title: 'Warehouses',
    singular: 'Warehouse',
    cols: [
      { key: 'name', label: 'Name', kind: 'main' },
      { key: 'code', label: 'Code', kind: 'mono' },
      { key: 'address', label: 'Address', kind: 'muted' },
      { key: 'phone', label: 'Phone', kind: 'muted' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'code', label: 'Code', type: 'text', required: true, mono: true },
      { key: 'address', label: 'Address', type: 'text' },
      { key: 'phone', label: 'Phone', type: 'text' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ],
  };
  readonly searchFields = ['name', 'code', 'address', 'phone'];
}
