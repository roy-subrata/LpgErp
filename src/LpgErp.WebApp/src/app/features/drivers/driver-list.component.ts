import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-driver-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [tabs]="tabs" [tabField]="tabField" [searchFields]="searchFields" />`,
})
export class DriverListComponent {
  readonly config: EntityConfig = {
    endpoint: 'drivers',
    title: 'Drivers',
    singular: 'Driver',
    cols: [
      { key: 'name', label: 'Name', kind: 'main', sub: 'licenseNumber' },
      { key: 'phone', label: 'Phone', kind: 'text' },
      { key: 'licenseNumber', label: 'License', kind: 'mono' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'phone', label: 'Phone', type: 'text' },
      { key: 'licenseNumber', label: 'License Number', type: 'text', mono: true },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ],
  };
  readonly tabs = [{ label: 'Active', value: 'true' }, { label: 'Inactive', value: 'false' }];
  readonly tabField = 'isActive';
  readonly searchFields = ['name', 'phone', 'licenseNumber'];
}
