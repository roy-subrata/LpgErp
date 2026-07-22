import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityListComponent, EntityConfig } from '../../shared/entity-list.component';

@Component({
  selector: 'app-truck-list',
  standalone: true,
  imports: [CommonModule, EntityListComponent],
  template: `<app-entity-list [config]="config" [searchFields]="searchFields" />`,
})
export class TruckListComponent {
  readonly config: EntityConfig = {
    endpoint: 'trucks',
    title: 'Trucks',
    singular: 'Truck',
    cols: [
      { key: 'registrationNumber', label: 'Registration', kind: 'main' },
      { key: 'name', label: 'Name', kind: 'text' },
      { key: 'phone', label: 'Phone', kind: 'text' },
      { key: 'isActive', label: 'Status', kind: 'badge', badgeMap: { true: ['Active', '#f0fdf4', '#15803d'], false: ['Inactive', '#f4f5f7', '#6b7280'] } },
    ],
    fields: [
      { key: 'registrationNumber', label: 'Registration Number', type: 'text', required: true, mono: true },
      { key: 'name', label: 'Name', type: 'text', required: true },
      { key: 'phone', label: 'Phone', type: 'text' },
      { key: 'isActive', label: 'Active', type: 'toggle' },
    ],
  };
  readonly searchFields = ['registrationNumber', 'name'];
}
