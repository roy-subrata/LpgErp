import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-cylinder-size-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class CylinderSizeListComponent {
  readonly config: EntityConfig = {
    endpoint: 'cylindersizes',
    title: 'Cylinder Sizes',
    singular: 'Cylinder Size',
    cols: [
      { key: 'name', label: 'Name', kind: 'main', sub: 'brandName' },
      { key: 'weightKg', label: 'Weight (kg)', kind: 'num' },
      { key: 'depositAmount', label: 'Deposit', kind: 'money' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'brandId', label: 'Brand', type: 'select', endpoint: 'brands', optionLabel: 'name' },
      { key: 'weightKg', label: 'Weight (kg)', type: 'number' },
      { key: 'depositAmount', label: 'Deposit Amount', type: 'number' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ],
  };
  readonly searchFields = ['name', 'brandName'];
}
